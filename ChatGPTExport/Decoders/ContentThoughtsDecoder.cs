using ChatGPTExport.Models;

namespace ChatGPTExport.Decoders
{
    public class ContentThoughtsDecoder : IDecoder<ContentThoughts, MarkdownContentResult>
    {
        public MarkdownContentResult Decode(ContentThoughts content, MessageContext context)
        {
            if (!context.ShowHidden)
            {
                return MarkdownContentResult.Empty();
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
            return MarkdownContentResult.FromLinesWithSuffix(markdownContent, " 💭");
        }
    }
}
