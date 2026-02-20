using ChatGPTExport.Models;

namespace ChatGPTExport.Decoders
{
    public class ContentThoughtsDecoder(bool showHidden)
    {
        public MarkdownContentResult DecodeToMarkdown(ContentThoughts content, MessageContext context)
        {
            if (!showHidden)
            {
                return new MarkdownContentResult();
            }

            var markdownContent = new List<string>();
            if (content.thoughts != null)
            {
                foreach (var thought in content.thoughts)
                {
                    markdownContent.Add(thought.summary + "  ");
                    markdownContent.Add(thought.content + "  ");
                }
            }
            return new MarkdownContentResult(markdownContent, " 💭");
        }
    }
}
