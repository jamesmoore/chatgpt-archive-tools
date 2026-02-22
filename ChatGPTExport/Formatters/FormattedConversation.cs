namespace ChatGPTExport.Formatters
{
    public record FormattedConversation(
        string Contents, 
        IEnumerable<IFormattedConversationAsset> Assets, 
        string Extension);
}
