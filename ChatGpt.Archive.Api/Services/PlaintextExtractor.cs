using ChatGPTExport.Assets;
using ChatGPTExport.Exporters;
using ChatGPTExport.Formatters.Plaintext;
using ChatGPTExport.Models;

namespace ChatGpt.Archive.Api.Services
{
    public class PlaintextExtractor
    {
        private readonly PlaintextFormatter plaintextFormatter = new(false);
        private readonly MarkdownContentVisitor visitor = new(new NullAssetLocator(), false);

        public string ExtractPlaintext(Message message)
        {
            var results = plaintextFormatter.FormatMessage(message, visitor);
            return string.Join(Environment.NewLine, results);
        }

        private class NullAssetLocator : IAssetLocator
        {
            public Asset? GetMarkdownMediaAsset(AssetRequest assetRequest)
            {
                return null;
            }
        }
    }
}
