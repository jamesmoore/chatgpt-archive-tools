using ChatGPTExport.Assets;
using ChatGPTExport.Decoders;
using ChatGPTExport.Decoders.AssetRenderer;
using ChatGPTExport.Formatters.Plaintext;
using ChatGPTExport.Models;
using ChatGPTExport.Visitor;

namespace ChatGpt.Archive.Api.Services
{
    public class PlaintextExtractor
    {
        private readonly FTSPlaintextMessageFormatter plaintextFormatter = new();
        private static readonly NullAssetRenderer markdownAssetRenderer = new();
        private readonly MarkdownContentVisitor visitor = new(new NullAssetLocator(),  markdownAssetRenderer);

        public string ExtractPlaintext(Message message, ConversationContext conversationContext)
        {
            var results = plaintextFormatter.FormatMessage(message, visitor, conversationContext).Where(p => string.IsNullOrWhiteSpace(p) == false);
            return string.Join(Environment.NewLine, results);
        }

        private class NullAssetRenderer : IMarkdownAssetRenderer
        {
            public string RenderAsset(FileSystemAsset? asset, string asset_pointer)
            {
                return string.Empty;
            }
        }

        private class NullAssetLocator : IFileSystemAssetLocator
        {
            public FileSystemAsset? GetMarkdownMediaAsset(FileSystemAssetRequest assetRequest) => null;
        }
    }
}
