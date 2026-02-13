namespace ChatGpt.Archive.Api.Database
{
    public class SearchResult
    {
        public string ConversationId { get; set; } = string.Empty;
        public string MessageId { get; set; } = string.Empty;
        public string Snippet { get; set; } = string.Empty;
        public string ConversationTitle { get; set; } = string.Empty;
    }
}
