namespace ChatGPTExport.Formatters
{
    public record FormattedConversation(string Contents, IEnumerable<ConversationAsset> Assets, string Extension);

    public record ConversationAsset(string Name, string Contents);
}
