using ChatGPTExport.Assets;
using System.IO.Abstractions;

namespace ChatGpt.Exporter.Cli.Assets
{
    internal class ExportAssetLocatorFactory()
    {
        public IAssetLocator GetAssetLocator(IEnumerable<ConversationAssets> conversationAssets, IDirectoryInfo destination)
        {
            var assetLocators = conversationAssets.Select(asset => new AssetLocator(asset) as IAssetLocator).ToList();
            var compositeAssetLocator = new CompositeAssetLocator(assetLocators);
            return compositeAssetLocator;
        }
    }
}
