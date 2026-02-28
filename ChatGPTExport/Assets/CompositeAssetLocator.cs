namespace ChatGPTExport.Assets
{
    public class CompositeAssetLocator(IEnumerable<IFileSystemAssetLocator> assetLocators) : IFileSystemAssetLocator
    {
        public FileSystemAsset? GetMarkdownMediaAsset(FileSystemAssetRequest assetRequest)
        {
            return assetLocators
                .Select(locator => locator.GetMarkdownMediaAsset(assetRequest))
                .FirstOrDefault(asset => asset != null);
        }
    }
}
