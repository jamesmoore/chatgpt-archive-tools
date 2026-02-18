namespace ChatGpt.Archive.Api.Services
{
    public interface IConversationAssetsCache
    {
        MediaAssetDefinition? FindMediaAsset(string searchPattern);
        string? GetMediaAssetPath(int index, string relativePath);
        void Reset();
    }

    public record MediaAssetDefinition(string Name, int RootId, string RelativePath);
}