using ChatGPTExport.Models;

namespace ChatGPTExport.Decoders
{
    public class ContentExecutionOutputDecoder : IDecoder<ContentExecutionOutput, MarkdownContentResult>
    {
        private readonly string LineBreak = Environment.NewLine;

        public MarkdownContentResult Decode(ContentExecutionOutput content, MessageContext context)
        {
            if (!context.ShowHidden)
            {
                return MarkdownContentResult.Empty();
            }

            if (content.text == null)
            {
                return MarkdownContentResult.Empty();
            }

            var code = ToCodeBlock(content.text);
            return MarkdownContentResult.FromLine(code);
        }

        private string ToCodeBlock(string code, string? language = null)
        {
            return $"```{language}{LineBreak}{code}{LineBreak}```";
        }
    }
}
