using ChatGPTExport.Decoders;
using ChatGPTExport.Formatters;
using ChatGPTExport.Formatters.Html;
using ChatGPTExport.Formatters.Html.Headers;
using ChatGPTExport.Models;
using ChatGPTExport.Visitor;
using Markdig;
using System.Net;
using System.Text.RegularExpressions;

namespace ChatGPTExport.Exporters.Html
{
    internal partial class HtmlFormatter(
        IHtmlFormatter formatter,
        IHeaderProvider headerProvider,
        bool showHidden) : IConversationFormatter
    {
        private readonly string LineBreak = Environment.NewLine;
        private readonly MarkdownPipeline MarkdownPipeline = CreatePipeline(formatter);
        private readonly EmbeddedResourceAsset CssAsset = new("/styles/tailwindcompiled.css", "ChatGPTExport.Formatters.Html.Templates.Styles.tailwindcompiled.css", "text/css");

        public FormattedConversation Format(IMarkdownAssetRenderer assetLocator, Conversation conversation, string pathPrefix)
        {
            var messages = conversation.GetMessagesWithContent();

            var strings = new List<(string MessageId, Author Author, string Content, bool HasImage)>();

            var visitor = new MarkdownContentVisitor(assetLocator, new ConversationContext(), showHidden);

            foreach (var message in messages)
            {
                try
                {
                    var visitResult = message.Accept(visitor);

                    if (message.author != null && visitResult != null && visitResult.Lines.Any() && message.id != null)
                    {
                        strings.Add((message.id, message.author, string.Join(LineBreak, visitResult.Lines), visitResult.HasImage));
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.Message);
                }
            }

            var htmlFragments = strings.Select(p => GetHtmlFragment(p.MessageId, p.Author, p.Content, p.HasImage, MarkdownPipeline)).ToList();

            var titleString = WebUtility.HtmlEncode(conversation.title ?? "No title");

            var metaHeaders = new Dictionary<string, string>
            {
                { "title", titleString }
            };
            if (conversation.conversation_id != null)
            {
                metaHeaders.Add("chatgpt_conversation_id", conversation.conversation_id);
            }
            if (conversation.gizmo_id != null)
            {
                metaHeaders.Add("chatgpt_gizmo_id", conversation.gizmo_id);
            }
            metaHeaders.Add("chatgpt_created", conversation.GetCreateTime().ToString("s"));
            metaHeaders.Add("chatgpt_updated", conversation.GetUpdateTime().ToString("s"));

            var body = new HeaderInput(htmlFragments, metaHeaders);

            var headers = headerProvider.GetHeaders(body);

            string html = formatter.FormatHtmlPage(
                new HtmlPage(titleString, [headers], htmlFragments), 
                pathPrefix);

            return new FormattedConversation(html, [CssAsset], ".html");
        }

        [GeneratedRegex("```(.*)")]
        private static partial Regex MarkdownCodeBlockRegex();
        private static (bool HasCode, List<string> Languages) GetLanguages(string markdown)
        {
            var codeBlockRegex = MarkdownCodeBlockRegex().Matches(markdown);
            var languages = codeBlockRegex.Where(p => p.Groups.Count > 1).
                Select(p => p.Groups[1].Value).
                Where(p => string.IsNullOrWhiteSpace(p) == false).
                Select(p => p.Trim()).
                Distinct(StringComparer.OrdinalIgnoreCase).
                Select(v => v.ToLowerInvariant()).
                ToList();
            return (codeBlockRegex.Count > 0, languages);
        }

        private HtmlFragment GetHtmlFragment(string messageId, Author author, string markdown, bool hasImage, MarkdownPipeline markdownPipeline)
        {
            var hasMath = false;

            if (markdown.Contains(@"\(") && markdown.Contains(@"\)") ||
                markdown.Contains(@"\[") && markdown.Contains(@"\]"))
            {
                var escaped = MarkdownMathConverter.ConvertBackslashMathToDollar(markdown);
                hasMath = markdown != escaped;
                markdown = escaped;
            }

            var id = $"<a id=\"msg-{WebUtility.HtmlEncode(messageId)}\"></a>";

            var html = id + Environment.NewLine + Markdown.ToHtml(markdown, markdownPipeline);

            var (HasCode, Languages) = GetLanguages(markdown);

            var fragment = new HtmlFragment(
                author.role == "user",
                html,
                HasCode,
                hasMath,
                hasImage,
                Languages);
            return fragment;
        }

        private static MarkdownPipeline CreatePipeline(IHtmlFormatter formatter)
        {
            var pipelineBuilder = GetMarkdownPipelineBuilder();
            var pipeline = pipelineBuilder.Build();
            return pipeline;
        }

        private static MarkdownPipelineBuilder GetMarkdownPipelineBuilder()
        {
            var pipelineBuilder = new MarkdownPipelineBuilder()
                //.UseAlertBlocks()
                //.UseAbbreviations()
                .UseAutoIdentifiers()
                //.UseCitations()
                //.UseCustomContainers()
                //.UseDefinitionLists()
                //.UseEmphasisExtras()
                //.UseFigures()
                //.UseFooters()
                .UseFootnotes()
                //.UseGridTables()
                .UseMathematics()
                .UseMediaLinks()
                .UsePipeTables()
                .UseListExtras()
                .UseTaskLists()
                //.UseDiagrams()
                .UseAutoLinks()
                .DisableHtml();
            //.UseGenericAttributes(); 
            return pipelineBuilder;
        }
    }
}
