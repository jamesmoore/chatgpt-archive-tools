using ChatGPTExport.Models;

namespace ChatGPTExport.Formatters
{
    public record ContentVisitorContext(
        Author Author, 
        DateTimeOffset? CreatedDate,
        DateTimeOffset? UpdatedDate,
        MessageMetadata MessageMetadata,
        string Recipient
        )
    {
        public string Role => Author.role ?? string.Empty;
    };
}
