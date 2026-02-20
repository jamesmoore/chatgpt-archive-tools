using ChatGPTExport.Decoders;
using ChatGPTExport.Models;
using ChatGPTExport.Visitor;

namespace ChatGPTExport.Formatters.Plaintext
{
    public class FTSPlaintextMessageFormatter()
    {
        private readonly string LineBreak = Environment.NewLine;

        public IEnumerable<string> FormatMessage(Message message, IContentVisitor<MarkdownContentResult> visitor)
        {
            var strings = new List<string>();
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
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }
            return strings;
        }
    }
}
