using ChatGPTExport.Assets;

namespace ChatGPTExport.Formatters
{
    public record FormattedConversation(
        string Contents,
        IEnumerable<IFormattedConversationAsset> Assets,
        IEnumerable<Asset> MarkdownAssets,
        string Extension);
}
