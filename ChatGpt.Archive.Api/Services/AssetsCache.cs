using ChatGPTExport.Formatters;
using System.Collections.Concurrent;

namespace ChatGpt.Archive.Api.Services
{
    public class AssetsCache
    {
        private readonly ConcurrentDictionary<string, IFormattedConversationAsset> cache = new();

        public IFormattedConversationAsset? Get(string key)
        {
            if (cache.TryGetValue(key, out var asset))
            {
                return asset;
            }
            return null;
        }

        public void Set(string key, IFormattedConversationAsset asset)
        {
            cache[key] = asset;
        }
    }
}
