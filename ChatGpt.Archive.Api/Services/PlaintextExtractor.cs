using ChatGPTExport.Exporters;
using ChatGPTExport.Formatters;
using ChatGPTExport.Formatters.Markdown;
using ChatGPTExport.Formatters.Plaintext;
using ChatGPTExport.Models;

namespace ChatGpt.Archive.Api.Services
{
    public class PlaintextExtractor
    {
        private readonly FTSPlaintextMessageFormatter plaintextFormatter = new();
        private readonly MarkdownContentVisitor visitor = new(new NullAssetLocator(), false);

        public string ExtractPlaintext(Message message)
        {
            var results = plaintextFormatter.FormatMessage(message, visitor).Where(p => string.IsNullOrWhiteSpace(p) == false);
            return string.Join(Environment.NewLine, results);
        }

        private class NullAssetLocator : IMarkdownAssetRenderer
        {
            public IEnumerable<string> RenderAsset(ContentVisitorContext context, string asset_pointer)
            {
                return [];
            }
        }
    }
}
