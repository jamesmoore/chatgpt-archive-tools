using ChatGPTExport.Formatters;
using ChatGPTExport.Models;

namespace ChatGPTExport.Decoders
{
    public class ContentTetherBrowsingDisplayDecoder(bool showHidden)
    {
        public MarkdownContentResult DecodeToMarkdown(ContentTetherBrowsingDisplay content, MessageContext context)
        {
            if (!showHidden)
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
