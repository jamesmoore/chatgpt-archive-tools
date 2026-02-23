using ChatGPTExport.Models;

namespace ChatGPTExport.Decoders
{
    public class ContentTetherBrowsingDisplayDecoder : IDecoder<ContentTetherBrowsingDisplay, MarkdownContentResult>
    {
        public MarkdownContentResult Decode(ContentTetherBrowsingDisplay content, MessageContext context)
        {
            if (!context.ShowHidden)
            {
                return new MarkdownContentResult();
            }

            if (content.result == null)
            {
                return new MarkdownContentResult();
            }
            var lines = new string?[] {
                content.result.Replace("\n", "  \n"),
                content.summary
            }.OfType<string>();
            return new MarkdownContentResult(lines);
        }
    }
}
