using ChatGPTExport.Assets;
using ChatGPTExport.Decoders;
using ChatGPTExport.Formatters.Plaintext;
using ChatGPTExport.Models;
using ChatGPTExport.Visitor;

namespace ChatGpt.Archive.Api.Services
{
    public class PlaintextExtractor
    {
        private readonly FTSPlaintextMessageFormatter plaintextFormatter = new();
        private static readonly NullAssetRenderer markdownAssetRenderer = new();
        private readonly MarkdownContentVisitor visitor = new(new NullAssetLocator(),  markdownAssetRenderer, new ConversationContext(), false);

        public string ExtractPlaintext(Message message)
        {
            var results = plaintextFormatter.FormatMessage(message, visitor).Where(p => string.IsNullOrWhiteSpace(p) == false);
            return string.Join(Environment.NewLine, results);
        }

        private class NullAssetRenderer : IMarkdownAssetRenderer
        {
            public IEnumerable<string> RenderAsset(Asset? asset, string asset_pointer)
            {
                return [];
            }
        }

        private class NullAssetLocator : IAssetLocator
        {
            public Asset? GetMarkdownMediaAsset(AssetRequest assetRequest) => null;
        }
    }
}
