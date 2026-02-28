using ChatGPTExport.Assets;

namespace ChatGpt.Archive.Api.Services
{
    public interface IConversationAssetsCache
    {
        void StoreAsset(FileSystemAsset asset);
        FileSystemAsset? GetAsset(string key);
        void Reset();
    }
}