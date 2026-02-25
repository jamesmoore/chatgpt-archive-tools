using ChatGPTExport.Assets;
using System.Collections.Concurrent;

namespace ChatGpt.Archive.Api.Services
{
    public class ConversationAssetsCache : IConversationAssetsCache
    {
        private ConcurrentDictionary<string, Asset> assetCache = new();

        public void StoreAsset(Asset asset)
        {
            var key = string.Join("/", asset.PathSegments.Skip(1));
            assetCache[key] = asset;
        }

        public Asset? GetAsset(string key)
        {
            var asset = assetCache.GetValueOrDefault(key);
            return asset;
        }

        public void Reset()
        {
            assetCache.Clear();
        }
    }
}
