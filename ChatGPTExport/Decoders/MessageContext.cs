using ChatGPTExport.Models;

namespace ChatGPTExport.Decoders
{
    public record MessageContext(
        Author Author,
        DateTimeOffset? CreatedDate,
        DateTimeOffset? UpdatedDate,
        MessageMetadata MessageMetadata,
        string Recipient,
        ConversationContext ConversationContext,
        bool ShowHidden
        )
    {
        public string Role => Author.role ?? string.Empty;
    };
}
