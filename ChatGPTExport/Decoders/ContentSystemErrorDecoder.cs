using ChatGPTExport.Models;

namespace ChatGPTExport.Decoders
{
    public class ContentSystemErrorDecoder(bool showHidden) : IDecodeTo<ContentSystemError, MarkdownContentResult>
    {
        public MarkdownContentResult DecodeTo(ContentSystemError content, MessageContext context)
        {
            if (!showHidden)
            {
                return new MarkdownContentResult();
            }

            return new MarkdownContentResult($"🔴 {content.name}: {content.text}");
        }
    }
}
