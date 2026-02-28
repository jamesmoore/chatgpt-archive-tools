using ChatGPTExport.Assets;

namespace ChatGpt.Archive.Api.Services
{
    public class ApiAssetLocator(IFileSystemAssetLocator inner, IConversationAssetsCache conversationAssetsCache) : IFileSystemAssetLocator
    {
        public FileSystemAsset? GetMarkdownMediaAsset(FileSystemAssetRequest assetRequest)
        {
            var asset = inner.GetMarkdownMediaAsset(assetRequest);
            if (asset == null)
            {
                return null;
            }
            else
            {
                var apiAsset = new FileSystemAsset(
                    asset.Name,
                    asset.FileInfo,
                    ["asset", .. asset.PathSegments],
                    asset.CreatedDate,
                    asset.UpdatedDate);

                conversationAssetsCache.StoreAsset(apiAsset);
                return apiAsset;
            }
        }
    }
}