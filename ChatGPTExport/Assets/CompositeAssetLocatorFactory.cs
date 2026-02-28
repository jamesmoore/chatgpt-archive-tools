namespace ChatGPTExport.Assets
{
    public class CompositeAssetLocatorFactory()
    {
        public IFileSystemAssetLocator GetAssetLocator(IEnumerable<ConversationAssets> conversationAssets)
        {
            var assetLocators = conversationAssets.Select(asset => new FileSystemAssetLocator(asset) as IFileSystemAssetLocator).ToList();
            var compositeAssetLocator = new CompositeAssetLocator(assetLocators);
            return compositeAssetLocator;
        }
    }
}
