using ChatGPTExport.Assets;
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
        IContentVisitor<MarkdownContentResult> markdownContentVisitor
        ) : IConversationFormatter
    {
        private readonly MarkdownPipeline MarkdownPipeline = CreatePipeline(formatter);
        private readonly IFormattedConversationAsset CssAsset = new EmbeddedResourceAsset("/styles/tailwindcompiled.css", "ChatGPTExport.Formatters.Html.Templates.Styles.tailwindcompiled.css", "text/css");

        public FormattedConversation Format(Conversation conversation, string pathPrefix, bool showHidden)
        {
            var messages = conversation.GetMessagesWithContent();

            var strings = new List<NewStruct>();
            var assets = new List<IFormattedConversationAsset>();
            ConversationContext conversationContext = new();

            foreach (var message in messages)
            {
                try
                {
                    var visitResult = message.Accept(markdownContentVisitor, conversationContext, showHidden);

                    if (visitResult != null)
                    {
                        if (message.author != null && visitResult.Lines.Any() && message.id != null)
                        {
                            strings.Add(new NewStruct(
                                message.id, 
                                message.author, 
                                visitResult.ToMarkdown(Environment.NewLine), 
                                visitResult.Lines.Any(p => p.Modifier == MarkdownModifier.Image),
                                visitResult.Lines.Any(p => p.Modifier == MarkdownModifier.Writing)
                                ));
                        }
                        assets.AddRange(visitResult.Assets);
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.Message);
                }
            }

            var htmlFragments = strings.Select(p => GetHtmlFragment(p)).ToList();

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
                new HtmlPage(titleString, [headers], htmlFragments, CssAsset.Name),
                pathPrefix);

            return new FormattedConversation(html, [CssAsset, .. assets], ".html");
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

        private HtmlFragment GetHtmlFragment(NewStruct newStruct)
        {
            var hasMath = false;
            var markdown = newStruct.Content;

            if (markdown.Contains(@"\(") && markdown.Contains(@"\)") ||
                markdown.Contains(@"\[") && markdown.Contains(@"\]"))
            {
                var escaped = MarkdownMathConverter.ConvertBackslashMathToDollar(markdown);
                hasMath = markdown != escaped;
                markdown = escaped;
            }

            var id = $"<a id=\"msg-{WebUtility.HtmlEncode(newStruct.MessageId)}\"></a>";

            var html = id + Environment.NewLine + Markdown.ToHtml(markdown, MarkdownPipeline);

            var (HasCode, Languages) = GetLanguages(markdown);

            var fragment = new HtmlFragment(
                newStruct.Author.role == "user",
                html,
                HasCode,
                hasMath,
                newStruct.HasImage,
                newStruct.HasWriting,
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

    internal record struct NewStruct(
        string MessageId,
        Author Author,
        string Content,
        bool HasImage,
        bool HasWriting
        );
}
