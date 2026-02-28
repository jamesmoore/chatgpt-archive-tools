using ChatGPTExport.Assets;

namespace ChatGpt.Archive.Api.Services
{
    /// <summary>
    /// Wraps an IFileSystemAssetLocator to convert the FileSystemAsset's PathSegments to include an "asset" segment at the beginning,
    /// so that the API can serve them from a consistent URL path like /asset/{conversationId}/{assetName}.
    /// </summary>
    /// <param name="inner"></param>
    public class ApiAssetLocator(IFileSystemAssetLocator inner) : IFileSystemAssetLocator
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
                return apiAsset;
            }
        }
    }
}