using ChatGPTExport.Exporters.Html;
using ChatGPTExport.Formatters;
using ChatGPTExport.Formatters.Html.Headers;
using ChatGPTExport.Formatters.Html.Template;
using ChatGPTExport.Formatters.Json;
using ChatGPTExport.Formatters.Markdown;
using ChatGPTExport.Formatters.Plaintext;

namespace ChatGPTExport
{
    public class ConversationFormatterFactory
    {
        public IEnumerable<IConversationFormatter> GetFormatters(
            IEnumerable<ExportType> exportTypes,
            bool showHidden
            )
        {
            var exporters = new List<IConversationFormatter>();
            if (exportTypes.Contains(ExportType.Json))
            {
                exporters.Add(new JsonFormatter());
            }
            if (exportTypes.Contains(ExportType.Markdown))
            {
                exporters.Add(new MarkdownFormatter(showHidden));
            }
            if (exportTypes.Contains(ExportType.Html))
            {
                var headerProvider = new CompositeHeaderProvider(
                    [
                        new MetaHeaderProvider(),
                        new HighlightHeaderProvider(),
                        new MathjaxHeaderProvider(),
                        new GlightboxHeaderProvider(),
                    ]
                );

                exporters.Add(new HtmlFormatter(new TailwindHtmlFormatter(), headerProvider, showHidden));
            }
            if (exportTypes.Contains(ExportType.Text))
            {
                exporters.Add(new PlaintextFormatter(showHidden));
            }

            return exporters;
        }
    }
}
