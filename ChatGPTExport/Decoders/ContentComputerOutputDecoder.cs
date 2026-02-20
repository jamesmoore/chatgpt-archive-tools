using ChatGPTExport.Formatters;
using ChatGPTExport.Models;

namespace ChatGPTExport.Decoders
{
    public class ContentComputerOutputDecoder
    {
        public MarkdownContentResult DecodeToMarkdown(ContentComputerOutput content, MessageContext context)
        {
            return new MarkdownContentResult();
        }
    }
}
