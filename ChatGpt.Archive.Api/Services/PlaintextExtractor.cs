using ChatGPTExport.Decoders;
using ChatGPTExport.Exporters;
using ChatGPTExport.Formatters;
using ChatGPTExport.Formatters.Plaintext;
using ChatGPTExport.Models;

namespace ChatGpt.Archive.Api.Services
{
    public class PlaintextExtractor
    {
        private readonly FTSPlaintextMessageFormatter plaintextFormatter = new();
        private readonly MarkdownContentVisitor visitor = new(
            new ContentTextDecoder(new ConversationContext(), false),
            new ContentMultimodalTextDecoder(new NullAssetLocator()),
            false);

        public string ExtractPlaintext(Message message)
        {
            var results = plaintextFormatter.FormatMessage(message, visitor).Where(p => string.IsNullOrWhiteSpace(p) == false);
            return string.Join(Environment.NewLine, results);
        }

        private class NullAssetLocator : IMarkdownAssetRenderer
        {
            public IEnumerable<string> RenderAsset(MessageContext context, string asset_pointer)
            {
                return [];
            }
        }
    }
}
