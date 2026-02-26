using ChatGPTExport.Models;
using System.Text.RegularExpressions;

namespace ChatGPTExport.Decoders
{
    public partial class ContentCodeDecoder : IDecoder<ContentCode, MarkdownContentResult>
    {
        private readonly string LineBreak = Environment.NewLine;

        public MarkdownContentResult Decode(ContentCode content, MessageContext context)
        {
            if (context.ShowHidden == false && context.Recipient != "all")
            {
                return MarkdownContentResult.Empty();
            }

            if (string.IsNullOrWhiteSpace(content.text))
            {
                return MarkdownContentResult.Empty();
            }

            var searchRegex = SearchRegex();
            var matches = searchRegex.Match(content.text);
            if (content.language == "unknown" && matches.Success)
            {
                var code = matches.Groups[1].Value;
                return MarkdownContentResult.FromLine($"> 🔍 **Web search:** {code}.");
            }
            else if (content.language == "unknown" && content.text.IsValidJson())
            {
                var code = ToCodeBlock(content.text, "json");
                return MarkdownContentResult.FromLine(code);
            }
            else
            {
                var code = ToCodeBlock(content.text, content.language);
                return MarkdownContentResult.FromLine(code);
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
