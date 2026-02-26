using ChatGPTExport.Models;
using ChatGPTExport.Validators;
using System.IO.Abstractions;
using System.Text.Json;

namespace ChatGPTExport
{
    public enum ConversationParseStatus
    {
        Success,
        ValidationFail,
        Error,
    }

    public record struct ConversationParseResult(ConversationParseStatus Status, Conversations? Conversations = null);

    public class ConversationsParser(IEnumerable<IConversationsValidator> validators)
    {
        private readonly JsonSerializerOptions options = new()
        {
            AllowOutOfOrderMetadataProperties = true,
        };

        public ConversationParseResult GetConversations(IFileInfo p)
        {
            try
            {
                Console.WriteLine($"Loading conversation " + p.FullName);
                return new ConversationParseResult(ConversationParseStatus.Success, this.GetConversationsForFile(p));
            }
            catch (ValidationException)
            {
                return new ConversationParseResult(ConversationParseStatus.ValidationFail);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error parsing file: {p.FullName}{Environment.NewLine}\t{ex.Message}");
                return new ConversationParseResult(ConversationParseStatus.Error);
            }
        }

        private Conversations GetConversationsForFile(IFileInfo sourceFile)
        {
            var conversationsJsonStream = sourceFile.FileSystem.File.OpenRead(sourceFile.FullName);

            var conversations = JsonSerializer.Deserialize<Conversations>(conversationsJsonStream, options);

            if (conversations == null)
            {
                throw new ApplicationException($"Conversations file {sourceFile.FullName} could not be deserialized");
            }

            if (validators.Any())
            {
                Console.WriteLine($"Validating: {sourceFile.FullName}");
                var results = validators.Select(p => p.Validate(conversationsJsonStream, conversations)).ToList();
                if (results.Any(p => p == false))
                {
                    throw new ValidationException();
                }
            }
            return conversations;
        }
    }
}
