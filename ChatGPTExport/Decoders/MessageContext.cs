using ChatGPTExport.Models;

namespace ChatGPTExport.Decoders
{
    public record MessageContext(
        Author Author,
        DateTimeOffset? CreatedDate,
        DateTimeOffset? UpdatedDate,
        MessageMetadata MessageMetadata,
        string Recipient)
    {
        public string Role => Author.role ?? string.Empty;
    };
}
