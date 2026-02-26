using ChatGPTExport.Models;

namespace ChatGPTExport.Decoders
{
    public class ContentTetherBrowsingDisplayDecoder : IDecoder<ContentTetherBrowsingDisplay, MarkdownContentResult>
    {
        public MarkdownContentResult Decode(ContentTetherBrowsingDisplay content, MessageContext context)
        {
            if (!context.ShowHidden)
            {
                return MarkdownContentResult.Empty();
            }

            if (content.result == null)
            {
                return MarkdownContentResult.Empty();
            }
            var lines = new string?[] {
                content.result.Replace("\n", "  \n"),
                content.summary
            }.OfType<string>();
            return MarkdownContentResult.FromLines(lines);
        }
    }
}
