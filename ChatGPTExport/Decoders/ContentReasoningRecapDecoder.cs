using ChatGPTExport.Models;

namespace ChatGPTExport.Decoders
{
    public class ContentReasoningRecapDecoder : IDecoder<ContentReasoningRecap, MarkdownContentResult>
    {
        public MarkdownContentResult Decode(ContentReasoningRecap content, MessageContext context)
        {
            if (!context.ShowHidden)
            {
                return MarkdownContentResult.Empty();
            }

            if (content.content == null)
            {
                return MarkdownContentResult.Empty();
            }
            return MarkdownContentResult.FromLine(content.content);
        }
    }
}
