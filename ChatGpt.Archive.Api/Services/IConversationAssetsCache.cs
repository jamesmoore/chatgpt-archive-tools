using ChatGPTExport.Assets;

namespace ChatGpt.Archive.Api.Services
{
    public interface IConversationAssetsCache
    {
        void StoreAsset(Asset asset);
        Asset? GetAsset(string key);
        void Reset();
    }
}