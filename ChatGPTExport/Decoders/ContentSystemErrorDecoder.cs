using ChatGPTExport.Models;

namespace ChatGPTExport.Decoders
{
    public class ContentSystemErrorDecoder(bool showHidden) : IDecoder<ContentSystemError, MarkdownContentResult>
    {
        public MarkdownContentResult Decode(ContentSystemError content, MessageContext context)
        {
            if (!showHidden)
            {
                return new MarkdownContentResult();
            }

            return new MarkdownContentResult($"🔴 {content.name}: {content.text}");
        }
    }
}
