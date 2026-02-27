namespace ChatGPTExport.Assets
{
    public class CompositeAssetLocatorFactory()
    {
        public IAssetLocator GetAssetLocator(IEnumerable<ConversationAssets> conversationAssets)
        {
            var assetLocators = conversationAssets.Select(asset => new AssetLocator(asset) as IAssetLocator).ToList();
            var compositeAssetLocator = new CompositeAssetLocator(assetLocators);
            return compositeAssetLocator;
        }
    }
}
