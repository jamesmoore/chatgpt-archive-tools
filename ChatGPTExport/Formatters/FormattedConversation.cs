using ChatGPTExport.Assets;

namespace ChatGPTExport.Formatters
{
    public record FormattedConversation(
        string Contents,
        IEnumerable<IFormattedConversationAsset> Assets,
        IEnumerable<FileSystemAsset> FileSystemAssets,
        string Extension);
}
