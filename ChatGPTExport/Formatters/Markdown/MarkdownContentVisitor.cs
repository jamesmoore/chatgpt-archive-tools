using ChatGPTExport.Decoders;
using ChatGPTExport.Formatters;
using ChatGPTExport.Formatters.Markdown;
using ChatGPTExport.Models;
using System.Text.RegularExpressions;

namespace ChatGPTExport.Exporters
{
    public partial class MarkdownContentVisitor(
        UnhandledContentDecoder unhandledContentDecoder,
        ContentTextDecoder contentTextDecoder,
        ContentMultimodalTextDecoder contentMultimodalTextDecoder,
        bool showHidden) : IContentVisitor<MarkdownContentResult>
    {
        private readonly string LineBreak = Environment.NewLine;

        /// <summary>
        /// Catch all for unhandled content types.
        /// </summary>
        /// <param name="content"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public MarkdownContentResult Visit(ContentBase content, MessageContext context)
        {
            return unhandledContentDecoder.DecodeToMarkdown(content, context);
        }

        public MarkdownContentResult Visit(ContentText content, MessageContext context)
        {
            return contentTextDecoder.DecodeToMarkdown(content, context);
        }

        public MarkdownContentResult Visit(ContentMultimodalText content, MessageContext context)
        {
            return contentMultimodalTextDecoder.DecodeToMarkdown(content, context);
        }

        public MarkdownContentResult Visit(ContentCode content, MessageContext context)
        {
            if (showHidden == false && context.Recipient != "all")
            {
                return new MarkdownContentResult();
            }

            if (string.IsNullOrWhiteSpace(content.text))
            {
                return new MarkdownContentResult();
            }

            var searchRegex = SearchRegex();
            var matches = searchRegex.Match(content.text);
            if (content.language == "unknown" && matches.Success)
            {
                var code = matches.Groups[1].Value;
                return new MarkdownContentResult($"> 🔍 **Web search:** {code}.");
            }
            else if (content.language == "unknown" && content.text.IsValidJson())
            {
                var code = ToCodeBlock(content.text, "json");
                return new MarkdownContentResult(code);
            }
            else
            {
                var code = ToCodeBlock(content.text, content.language);
                return new MarkdownContentResult(code);
            }
        }

        private string ToCodeBlock(string code, string? language = null)
        {
            return $"```{language}{LineBreak}{code}{LineBreak}```";
        }

        public MarkdownContentResult Visit(ContentThoughts content, MessageContext context)
        {
            return GetShowAllGuardedContentResult(() =>
            {
                var markdownContent = new List<string>();
                if (content.thoughts != null)
                {
                    foreach (var thought in content.thoughts)
                    {
                        markdownContent.Add(thought.summary + "  ");
                        markdownContent.Add(thought.content + "  ");
                    }
                }
                return new MarkdownContentResult(markdownContent, " 💭");
            });
        }

        public MarkdownContentResult Visit(ContentExecutionOutput content, MessageContext context)
        {
            return GetShowAllGuardedContentResult(() =>
            {
                if (content.text == null)
                {
                    return new MarkdownContentResult();
                }

                var code = ToCodeBlock(content.text);
                return new MarkdownContentResult(code);
            });
        }

        public MarkdownContentResult Visit(ContentReasoningRecap content, MessageContext context)
        {
            return GetShowAllGuardedContentResult(() =>
            {
                if (content.content == null)
                {
                    return new MarkdownContentResult();
                }
                return new MarkdownContentResult(content.content);
            });
        }

        [GeneratedRegex("""^search\("(.*)"\)$""")]
        private static partial Regex SearchRegex();

        public MarkdownContentResult Visit(ContentUserEditableContext content, MessageContext context)
        {
            return GetShowAllGuardedContentResult(() =>
            {
                var markdownContent = new List<string>
                {
                    "**User profile:** " + content.user_profile + "  ",
                    "**User instructions:** " + content.user_instructions + "  "
                };
                return new MarkdownContentResult(markdownContent);
            });
        }

        public MarkdownContentResult Visit(ContentTetherBrowsingDisplay content, MessageContext context)
        {
            return GetShowAllGuardedContentResult(() =>
            {
                if (content.result == null)
                {
                    return new MarkdownContentResult();
                }
                var lines = new string?[] {
                    content.result.Replace("\n", "  \n"),
                    content.summary
                }.OfType<string>();
                return new MarkdownContentResult(lines);
            });
        }

        public MarkdownContentResult Visit(ContentComputerOutput content, MessageContext context)
        {
            return new MarkdownContentResult();
        }

        public MarkdownContentResult Visit(ContentSystemError content, MessageContext context)
        {
            return GetShowAllGuardedContentResult(() =>
            {
                return new MarkdownContentResult($"🔴 {content.name}: {content.text}");
            });
        }

        private MarkdownContentResult GetShowAllGuardedContentResult(Func<MarkdownContentResult> func)
        {
            return showHidden ? func() : new MarkdownContentResult();
        }
    }
}
