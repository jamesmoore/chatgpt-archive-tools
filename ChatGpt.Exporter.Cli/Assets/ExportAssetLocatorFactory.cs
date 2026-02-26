using ChatGPTExport.Assets;

namespace ChatGpt.Exporter.Cli.Assets
{
    internal class ExportAssetLocatorFactory()
    {
        public IAssetLocator GetAssetLocator(IEnumerable<ConversationAssets> conversationAssets)
        {
            var assetLocators = conversationAssets.Select(asset => new AssetLocator(asset) as IAssetLocator).ToList();
            var compositeAssetLocator = new CompositeAssetLocator(assetLocators);
            return compositeAssetLocator;
        }
    }
}
