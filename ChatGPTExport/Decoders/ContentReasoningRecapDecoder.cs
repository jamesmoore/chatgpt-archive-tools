using ChatGPTExport.Models;

namespace ChatGPTExport.Decoders
{
    public class ContentReasoningRecapDecoder(bool showHidden) : IDecodeTo<ContentReasoningRecap, MarkdownContentResult>
    {
        public MarkdownContentResult DecodeTo(ContentReasoningRecap content, MessageContext context)
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
