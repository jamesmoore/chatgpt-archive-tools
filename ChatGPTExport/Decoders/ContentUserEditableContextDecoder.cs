using ChatGPTExport.Models;

namespace ChatGPTExport.Decoders
{
    public class ContentUserEditableContextDecoder : IDecoder<ContentUserEditableContext, MarkdownContentResult>
    {
        public MarkdownContentResult Decode(ContentUserEditableContext content, MessageContext context)
        {
            if (!context.ShowHidden)
            {
                return MarkdownContentResult.Empty();
            }

            var markdownContent = new List<string>
            {
                "**User profile:** " + content.user_profile + "  ",
                "**User instructions:** " + content.user_instructions + "  "
            };
            return MarkdownContentResult.FromLines(markdownContent);
        }
    }
}
