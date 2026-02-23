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
                return new MarkdownContentResult();
            }

            if (content.text == null)
            {
                return new MarkdownContentResult();
            }

            var code = ToCodeBlock(content.text);
            return new MarkdownContentResult(code);
        }

        private string ToCodeBlock(string code, string? language = null)
        {
            return $"```{language}{LineBreak}{code}{LineBreak}```";
        }
    }
}
