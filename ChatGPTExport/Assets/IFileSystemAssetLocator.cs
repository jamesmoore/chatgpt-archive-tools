
namespace ChatGPTExport.Assets
{
    public interface IFileSystemAssetLocator
    {
        FileSystemAsset? GetMarkdownMediaAsset(FileSystemAssetRequest assetRequest);
    }
}