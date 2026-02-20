using ChatGPTExport.Models;

namespace ChatGPTExport.Decoders
{
    public class ContentComputerOutputDecoder : IDecodeTo<ContentComputerOutput, MarkdownContentResult>
    {
        public MarkdownContentResult DecodeTo(ContentComputerOutput content, MessageContext context)
        {
            return new MarkdownContentResult();
        }
    }
}
