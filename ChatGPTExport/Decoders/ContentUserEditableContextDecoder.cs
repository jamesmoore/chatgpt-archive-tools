using ChatGPTExport.Models;

namespace ChatGPTExport.Decoders
{
    public class ContentUserEditableContextDecoder(bool showHidden) : IDecoder<ContentUserEditableContext, MarkdownContentResult>
    {
        public MarkdownContentResult Decode(ContentUserEditableContext content, MessageContext context)
        {
            if (!showHidden)
            {
                return new MarkdownContentResult();
            }

            var markdownContent = new List<string>
            {
                "**User profile:** " + content.user_profile + "  ",
                "**User instructions:** " + content.user_instructions + "  "
            };
            return new MarkdownContentResult(markdownContent);
        }
    }
}
