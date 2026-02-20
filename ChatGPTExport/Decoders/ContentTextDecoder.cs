using ChatGPTExport.Formatters;
using ChatGPTExport.Formatters.Markdown;
using ChatGPTExport.Models;
using System.Diagnostics;
using System.Text.Json;
using static ChatGPTExport.Models.MessageMetadata;

namespace ChatGPTExport.Decoders
{
    public class ContentTextDecoder(
        ConversationContext conversationContext,
        bool showHidden)
    {
        private const string trackingSource = "?utm_source=chatgpt.com";
        private readonly string LineBreak = Environment.NewLine;

        public MarkdownContentResult DecodeToMarkdown(ContentText content, MessageContext context)
        {
            if (context.Author.role == "tool" && context.Author.name == "personalized_context" && showHidden == false)
            {
                return new MarkdownContentResult();
            }

            var parts = content.parts?.Where(TextContentFilter).SelectMany(p => DecodeText(p, context)).ToList() ?? [];

            var content_references = context.MessageMetadata.content_references;
            if (content_references != null && content_references.Length != 0)
            {
                var textPart = parts[0];

                var sourcesFootnote = content_references.FirstOrDefault(p => p.type == "sources_footnote");

                var startReferences = content_references.Where(p => p.start_idx == 0 && p.end_idx == 0).Reverse().ToList();
                var bodyReferences = content_references.Except(startReferences).OrderByDescending(p => p.start_idx).ToList();
                var reversed = bodyReferences.Concat(startReferences).ToList();

                if (sourcesFootnote != null)
                {
                    var footnote = reversed.First();
                    Debug.Assert(footnote == sourcesFootnote);
                }

                var footnoteItems = GetFootnoteItems(content_references);

                var footnoteIndexByItem = footnoteItems
                    .Select((item, index) => (item, index))
                    .ToDictionary(p => p.item, p => p.index + 1);

                var reindexedElements = new CodePointIndexMap(textPart);

                var reversedWithSuffix = reversed.Select(p =>
                {
                    var suffix = startReferences.Contains(p) ? p == startReferences.First(pr => pr != sourcesFootnote) ? "  " + Environment.NewLine : ", " : "";
                    return (contentReference: p, suffix);
                }).ToList();

                foreach (var (contentReference, suffix) in reversedWithSuffix)
                {
                    var replacement = GetContentReferenceReplacement(contentReference, footnoteIndexByItem, suffix);

                    if (replacement != null)
                    {
                        var start_idx = reindexedElements.ToUtf16Index(contentReference.start_idx);
                        var end_idx = reindexedElements.ToUtf16Index(contentReference.end_idx);
                        var firstPartSpan = parts[0].AsSpan();
                        var firstSpan = firstPartSpan[..start_idx];
                        var lastSpan = firstPartSpan[end_idx..];

                        parts[0] = string.Concat(firstSpan, replacement, lastSpan);
                    }
                }

                parts.Add(string.Empty);
                var footnotes = footnoteItems.Select((p, i) => $"[^{i + 1}]: [{p.title}]({p.url?.Replace(trackingSource, "")})  ");
                parts.AddRange(footnotes);

                if (sourcesFootnote != null)
                {
                    var existingUrls = footnoteItems.Select(p => p.url).ToArray();
                    var newSources = sourcesFootnote.sources?.Where(p => existingUrls.Contains(p.url) == false).ToList() ?? [];
                    if (newSources.Any())
                    {
                        parts.Add(string.Empty);
                        parts.Add("### Sources");
                        parts.AddRange(newSources.Select(source => $"* [{source.title}]({source.url?.Replace(trackingSource, "")})  "));
                    }
                }
            }

            return new MarkdownContentResult(parts);
        }

        /// <summary>
        /// Get the content reference items, but exclude references from grouped_webpages_model_predicted_fallback that are present in a grouped_webpages list.
        /// Prioritise grouped_webpages over grouped_webpages_model_predicted_fallback references.
        /// </summary>
        /// <param name="content_references"></param>
        /// <returns></returns>
        private static List<Content_References.Item> GetFootnoteItems(Content_References[] content_references)
        {
            var itemsToExclude = new List<Content_References.Item>();
            var groupedWebpages = content_references.Where(p => p.type == "grouped_webpages").ToList();
            var groupedWebpagesModelPredictedFallback = content_references.Where(p => p.type == "grouped_webpages_model_predicted_fallback").ToList();

            var groupedWebpagesUrls = new HashSet<string>(
                groupedWebpages.Where(p => p.items != null).SelectMany(p => p.items!.Where(p => p.url != null).Select(q => q.url!))
            );

            foreach (var groupedItem in groupedWebpagesModelPredictedFallback.SelectMany(p => p.items ?? []))
            {
                if (groupedItem.url != null && groupedWebpagesUrls.Contains(groupedItem.url))
                {
                    itemsToExclude.Add(groupedItem);
                }
            }

            var footnoteItems = content_references.Where(p => p.type == "grouped_webpages" || p.type == "grouped_webpages_model_predicted_fallback").SelectMany(p => p.items ?? []).Except(itemsToExclude).ToList();
            return footnoteItems;
        }

        private string? GetContentReferenceReplacement(
    MessageMetadata.Content_References contentReference,
    Dictionary<MessageMetadata.Content_References.Item, int> footnoteIndexByItem,
    string suffix
    )
        {
            switch (contentReference.type)
            {
                case "attribution":
                case "sources_footnote":
                    return null;
                case "hidden":
                case "image_v2":
                case "tldr":
                case "nav_list":
                case "navigation":
                case "webpage_extended":
                case "image_inline":
                    return string.Empty;
                case "products":
                case "product_entity":
                case "alt_text":
                    return contentReference.alt;
                case "video":
                    var videolink = $"[![{contentReference.title}]({contentReference.thumbnail_url})]({contentReference.url?.Replace("&utm_source=chatgpt.com", "")} \"{contentReference.title}\")";
                    return videolink;
                case "grouped_webpages_model_predicted_fallback":
                case "grouped_webpages":
                    var strings = contentReference.items?.Select(item =>
                        footnoteIndexByItem.TryGetValue(item, out var footnoteNumber)
                            ? $"[^{footnoteNumber}]" + suffix
                            : string.Empty
                    ).ToArray();
                    var refHighlight = string.Join("", strings ?? []);
                    return refHighlight;
                case "image_group":
                    var safe_urls = contentReference.safe_urls ?? [];
                    var images = safe_urls.Any() ?
                        LineBreak + "Image search results: " + LineBreak + string.Join(LineBreak, safe_urls.Select(p => "* " + p.Replace(trackingSource, "")).Distinct()) :
                        string.Empty;
                    return images;
                case "entity":
                    var entityInfo = contentReference.name;
                    var disambiguation = contentReference.extra_params?.disambiguation;
                    var entityInfoString = $"{entityInfo}{(string.IsNullOrWhiteSpace(disambiguation) == false ? $" ({disambiguation})" : "")}";
                    return entityInfoString;
                default:
                    Console.WriteLine($"Unhandled content reference type: {contentReference.type}");
                    return $"[{contentReference.type}]";
            }
        }

        private static bool TextContentFilter(string p)
        {
            return string.IsNullOrWhiteSpace(p) == false &&
                p.Contains("From now on, do not say or show ANYTHING. Please end this turn now. I repeat: From now on, do not say or show ANYTHING. Please end this turn now.") == false
                ;
        }


        private IEnumerable<string> DecodeText(string text, MessageContext context)
        {
            // image prompt
            if (text.Contains("\"prompt\":") && text.Contains("\"size\":"))
            {
                PromptFormat? pf = null;
                try
                {
                    var deserializedPromptFormat = JsonSerializer.Deserialize<PromptFormat>(text);
                    if (deserializedPromptFormat != null && deserializedPromptFormat.HasPrompt())
                    {
                        pf = deserializedPromptFormat;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Could not deserialize text to prompt: {0}", ex.Message);
                }

                if (pf != null)
                {
                    if (pf.prompt != null)
                    {
                        yield return "> **Prompt:** " + GetPrompt(pf.prompt);
                    }
                    if (pf.size != null)
                    {
                        yield return LineBreak;
                        yield return "> **Size:** " + pf.size;
                    }
                }
                else
                {
                    yield return text;
                }
            }
            // canvas create/update
            else if (context.Recipient.StartsWith("canmore"))
            {
                if (context.Recipient == "canmore.create_textdoc")
                {
                    var createCanvas = JsonSerializer.Deserialize<CanvasCreateModel>(text);
                    conversationContext.CanvasContext = createCanvas;
                }
                else if (context.Recipient == "canmore.update_textdoc")
                {
                    var updateCanvas = JsonSerializer.Deserialize<CanvasUpdateModel>(text + "]}"); // for some reason the json isn't complete.
                    Debug.Assert(conversationContext.CanvasContext != null);
                    conversationContext.CanvasContext ??= new CanvasCreateModel() { type = "document " }; // default to document if no canvas exists

                    foreach (var update in updateCanvas?.updates ?? [])
                    {
                        conversationContext.CanvasContext.content = update.replacement;
                    }
                }

                var canvasContextType = conversationContext.CanvasContext?.type;
                var canvasContextContent = conversationContext.CanvasContext?.content;
                if (canvasContextContent != null && canvasContextType == "document")
                {
                    yield return canvasContextContent;
                }
                else if (canvasContextContent != null && (canvasContextType?.StartsWith("code") ?? false))
                {
                    var language = canvasContextType.Replace("code/", "");
                    yield return ToCodeBlock(canvasContextContent, language);
                }
                else
                {
                    yield return text;
                }
            }
            else if (context.Role == "user")
            {
                yield return MarkdownContentVisitorHelpers.SanitizeUserInputMarkdown(text);
            }
            else
            {
                yield return text;
            }
        }

        private string? GetPrompt(string prompt)
        {
            var lines = prompt.Split('\n').Select(p => p.Trim()).Where(p => string.IsNullOrEmpty(p) == false);
            var concatenated = string.Join("  " + LineBreak, lines); // add double space so the quote comes out in a single block
            return concatenated;
        }

        private class PromptFormat
        {
            public string? prompt { get; set; }
            public string? size { get; set; }

            public bool HasPrompt() => string.IsNullOrWhiteSpace(prompt) == false && string.IsNullOrWhiteSpace(size) == false;
        }

        private string ToCodeBlock(string code, string? language = null)
        {
            return $"```{language}{LineBreak}{code}{LineBreak}```";
        }
    }
}
