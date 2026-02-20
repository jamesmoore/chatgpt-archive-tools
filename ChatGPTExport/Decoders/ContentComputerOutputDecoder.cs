using ChatGPTExport.Models;

namespace ChatGPTExport.Decoders
{
    public class ContentComputerOutputDecoder : IDecoder<ContentComputerOutput, MarkdownContentResult>
    {
        public MarkdownContentResult Decode(ContentComputerOutput content, MessageContext context)
        {
            return new MarkdownContentResult();
        }
    }
}
