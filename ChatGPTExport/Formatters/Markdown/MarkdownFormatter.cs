using ChatGPTExport.Assets;
using ChatGPTExport.Decoders;
using ChatGPTExport.Models;
using ChatGPTExport.Visitor;

namespace ChatGPTExport.Formatters.Markdown
{
    internal class MarkdownFormatter(IContentVisitor<MarkdownContentResult> markdownContentVisitor) : IConversationFormatter
    {
        public FormattedConversation Format(Conversation conversation, string pathPrefix, bool showHidden)
        {
            var messages = conversation.GetMessagesWithContent();

            var strings = new List<string>();
            var assets = new List<FileSystemAsset>();
            ConversationContext conversationContext = new();

            strings.AddRange(GetYamlHeader(conversation));

            foreach (var message in messages)
            {
                try
                {
                    var visitResult = message.Accept(markdownContentVisitor, conversationContext, showHidden);

                    if (visitResult != null)
                    {
                        if (message.author != null && visitResult.Lines.Any())
                        {
                            var authorname = string.IsNullOrWhiteSpace(message.author.name) ? "" : $" ({message.author.name})";
                            strings.Add($"**{message.author.role}{authorname}{visitResult.Suffix}**:  "); // double space for line break

                            strings.Add(FormatMarkdownLines(visitResult.Lines));
                            strings.Add(Environment.NewLine);
                        }
                        assets.AddRange(visitResult.Assets);
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.Message);
                }
            }

            return new FormattedConversation(string.Join(Environment.NewLine, strings), assets, ".md");
        }

        private static IEnumerable<string> GetYamlHeader(Conversation conversation)
        {
            return [
                "---",
                "chatgpt:",
                "  conversation_id: " + conversation.conversation_id,
                "  gizmo_id: " + conversation.gizmo_id,
                "  created: " + conversation.GetCreateTime().ToString("s"),
                "  updated: " + conversation.GetUpdateTime().ToString("s"),
                "title: " + conversation.title,
                "---",
            ];
        }

        private static string FormatMarkdownLines(IEnumerable<MarkdownContentLine> lines)
        {
            var formattedLines = new List<string>();

            foreach (var line in lines)
            {
                if (line.Modifier == MarkdownModifier.Writing)
                {
                    formattedLines.Add("*Writing:*  ");
                }

                formattedLines.Add(line.MarkdownContent);
            }

            return string.Join(Environment.NewLine, formattedLines);
        }
    }
}
