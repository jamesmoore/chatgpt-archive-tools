using ChatGPTExport.Assets;

namespace ChatGPTExport.Formatters
{
    public record FormattedConversation(
        string Contents,
        IEnumerable<IFormattedConversationAsset> Assets,
        IEnumerable<IFormattedConversationAsset> FileSystemAssets,
        string Extension)
    {
        public IEnumerable<IFormattedConversationAsset> AllAssets => Assets.Concat(FileSystemAssets);
    };
}
