using ChatGPTExport.Models;

namespace ChatGPTExport.Decoders
{
    public class ContentSystemErrorDecoder(bool showHidden)
    {
        public MarkdownContentResult DecodeToMarkdown(ContentSystemError content, MessageContext context)
        {
            if (!showHidden)
            {
                return new MarkdownContentResult();
            }

            return new MarkdownContentResult($"🔴 {content.name}: {content.text}");
        }
    }
}
