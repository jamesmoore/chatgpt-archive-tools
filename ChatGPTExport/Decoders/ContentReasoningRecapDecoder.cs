using ChatGPTExport.Formatters;
using ChatGPTExport.Models;

namespace ChatGPTExport.Decoders
{
    public class ContentReasoningRecapDecoder(bool showHidden)
    {
        public MarkdownContentResult DecodeToMarkdown(ContentReasoningRecap content, MessageContext context)
        {
            if (!showHidden)
            {
                return new MarkdownContentResult();
            }

            if (content.content == null)
            {
                return new MarkdownContentResult();
            }
            return new MarkdownContentResult(content.content);
        }
    }
}
