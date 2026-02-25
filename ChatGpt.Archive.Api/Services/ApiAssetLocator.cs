using ChatGPTExport.Assets;

namespace ChatGpt.Archive.Api.Services
{
    public class ApiAssetLocator(IAssetLocator inner, IConversationAssetsCache conversationAssetsCache) : IAssetLocator
    {
        public Asset? GetMarkdownMediaAsset(AssetRequest assetRequest)
        {
            var asset = inner.GetMarkdownMediaAsset(assetRequest);
            if (asset == null)
            {
                return null;
            }
            else
            {
                var apiAsset = new Asset(
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