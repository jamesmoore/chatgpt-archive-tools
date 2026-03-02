namespace ChatGPTExport
{
    public class ParsedConversationDirectoryFactory(ConversationsParser conversationsParser)
    {
        public async Task<ParsedConversationDirectories> CreateAsync(IEnumerable<ConversationExportDirectory> conversationExportDirectories)
        {
            var parsedDirectories = await Task.WhenAll(conversationExportDirectories.Select(CreateAsync));
            return new ParsedConversationDirectories(parsedDirectories);
        }

        private async Task<ParsedConversationDirectory> CreateAsync(ConversationExportDirectory conversationExportDirectory)
        {
            var parsedFiles = await Task.WhenAll(conversationExportDirectory.ConversationFiles.Select(async file =>
            {
                var parseResult = await conversationsParser.GetConversationsAsync(file);
                return new ParsedConversationFile
                {
                    File = file,
                    Conversations = parseResult.Conversations,
                    ParseStatus = parseResult.Status,
                };
            }));
            return new ParsedConversationDirectory
            {
                DirectoryInfo = conversationExportDirectory.DirectoryInfo,
                ParsedConversationFiles = parsedFiles,
            };
        }
    }
}
