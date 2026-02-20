using ChatGPTExport.Models;
using System.Text.RegularExpressions;

namespace ChatGPTExport.Decoders
{
    public partial class ContentCodeDecoder(bool showHidden) : IDecodeTo<ContentCode, MarkdownContentResult>
    {
        private readonly string LineBreak = Environment.NewLine;

        public MarkdownContentResult DecodeTo(ContentCode content, MessageContext context)
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

        [GeneratedRegex("""^search\("(.*)"\)$""")]
        private static partial Regex SearchRegex();
    }
}
