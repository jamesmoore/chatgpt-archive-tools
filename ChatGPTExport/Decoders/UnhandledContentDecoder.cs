using ChatGPTExport.Formatters;
using ChatGPTExport.Formatters.Markdown;
using ChatGPTExport.Models;

namespace ChatGPTExport.Decoders
{
    public class UnhandledContentDecoder
    {
        public MarkdownContentResult DecodeToMarkdown(ContentBase content, MessageContext context)
        {
            string name = content.GetType().Name;
            Console.WriteLine("\tUnhandled content type: " + name);

            var lines = new List<string>
            {
                $"Unhandled content type: {content}"
            };

            if (content.ExtraData != null && content.ExtraData.Count != 0)
            {
                lines.Add("|Name|Value|");
                lines.Add("|---|---|");
                foreach (var item in content.ExtraData.Take(1))
                {
                    lines.Add("|" + item.Key + "|" + item.Value.GetRawText().Replace("\\n", "<br>") + "|");
                }
            }

            return new MarkdownContentResult(lines);
        }
    }
}
