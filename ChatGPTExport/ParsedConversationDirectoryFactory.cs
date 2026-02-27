namespace ChatGPTExport
{
    public class ParsedConversationDirectoryFactory(ConversationsParser conversationsParser)
    {
        public ParsedConversationDirectories Create(IEnumerable<ConversationExportDirectory> conversationExportDirectories)
        {
            var parsedDirectories = conversationExportDirectories.Select(Create).ToList();
            return new ParsedConversationDirectories(parsedDirectories);
        }

        public ParsedConversationDirectory Create(ConversationExportDirectory conversationExportDirectory)
        {
            var parsedFiles = conversationExportDirectory.ConversationFiles.Select(file =>
            {
                var parseResult = conversationsParser.GetConversations(file);
                return new ParsedConversationFile
                {
                    File = file,
                    Conversations = parseResult.Conversations,
                    ParseStatus = parseResult.Status,
                };
            }).ToList();
            return new ParsedConversationDirectory
            {
                DirectoryInfo = conversationExportDirectory.DirectoryInfo,
                ParsedConversationFiles = parsedFiles,
            };
        }
    }
}
