using ChatGPTExport.Models;

namespace ChatGPTExport.Decoders
{
    public class ContentReasoningRecapDecoder(bool showHidden) : IDecoder<ContentReasoningRecap, MarkdownContentResult>
    {
        public MarkdownContentResult Decode(ContentReasoningRecap content, MessageContext context)
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
