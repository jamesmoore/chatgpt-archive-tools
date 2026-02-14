namespace ChatGpt.Archive.Api.Services
{
    public class ConsolidatedSearchResult
    {
        public string ConversationId { get; set; } = string.Empty;

        public string ConversationTitle { get; set; } = string.Empty;

        public List<MessageResult> Messages { get; set; } = new List<MessageResult>();

        public class MessageResult
        {
            public string MessageId { get; set; } = string.Empty;
            public string Snippet { get; set; } = string.Empty;
        }
    }
}
