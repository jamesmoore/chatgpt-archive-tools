using ChatGPTExport.Models;

namespace ChatGPTExport.Decoders
{
    public class ContentReasoningRecapDecoder : IDecoder<ContentReasoningRecap, MarkdownContentResult>
    {
        public MarkdownContentResult Decode(ContentReasoningRecap content, MessageContext context)
        {
            if (!context.ShowHidden)
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
