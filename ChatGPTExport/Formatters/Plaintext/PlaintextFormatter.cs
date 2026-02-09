using ChatGPTExport.Assets;
using ChatGPTExport.Exporters;
using ChatGPTExport.Formatters;
using ChatGPTExport.Models;
using Markdig;

namespace ChatGPTExport.Formatters.Plaintext
{
    internal class PlaintextFormatter(bool showHidden) : IConversationFormatter
    {
        private readonly string LineBreak = Environment.NewLine;

        public IEnumerable<string> Format(IAssetLocator assetLocator, Conversation conversation)
        {
            var messages = conversation.GetMessagesWithContent();

            var strings = new List<string>();

            var visitor = new MarkdownContentVisitor(assetLocator, showHidden);

            // Add conversation header
            strings.Add($"Title: {conversation.title ?? "No title"}");
            strings.Add($"Created: {conversation.GetCreateTime():yyyy-MM-dd HH:mm:ss}");
            strings.Add($"Updated: {conversation.GetUpdateTime():yyyy-MM-dd HH:mm:ss}");
            if (!string.IsNullOrEmpty(conversation.conversation_id))
            {
                strings.Add($"Conversation ID: {conversation.conversation_id}");
            }
            if (!string.IsNullOrEmpty(conversation.gizmo_id))
            {
                strings.Add($"Gizmo ID: {conversation.gizmo_id}");
            }
            strings.Add("");
            strings.Add("=" + new string('=', 78));
            strings.Add("");

            foreach (var message in messages)
            {
                try
                {
                    var visitResult = message.Accept(visitor);

                    if (message.author != null && visitResult != null && visitResult.Lines.Any())
                    {
                        var markdown = string.Join(LineBreak, visitResult.Lines);
                        
                        // Convert markdown to plaintext
                        var plaintext = Markdig.Markdown.ToPlainText(markdown);

                        var authorname = string.IsNullOrWhiteSpace(message.author.name) ? "" : $" ({message.author.name})";
                        strings.Add($"{message.author.role}{authorname}{visitResult.Suffix}:");
                        strings.Add(plaintext);
                        strings.Add("-" + new string('-', 78));
                        strings.Add("");
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.Message);
                }
            }

            return strings;
        }

        public string GetExtension() => ".txt";
    }
}
