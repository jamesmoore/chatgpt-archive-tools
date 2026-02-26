using ChatGPTExport.Models;

namespace ChatGPTExport.Decoders
{
    public class ContentSystemErrorDecoder : IDecoder<ContentSystemError, MarkdownContentResult>
    {
        public MarkdownContentResult Decode(ContentSystemError content, MessageContext context)
        {
            if (!context.ShowHidden)
            {
                return MarkdownContentResult.Empty();
            }

            return MarkdownContentResult.FromLine($"🔴 {content.name}: {content.text}");
        }
    }
}
