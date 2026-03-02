namespace ChatGPTExport
{
    public class ParsedConversationDirectoryFactory(ConversationsParser conversationsParser)
    {
        public async Task<ParsedConversationDirectories> CreateAsync(IEnumerable<ConversationExportDirectory> conversationExportDirectories)
        {
            var parsedDirectories = new List<ParsedConversationDirectory>();
            foreach (var conversationExportDirectory in conversationExportDirectories)
            {
                var item = await CreateAsync(conversationExportDirectory);
                foreach (var existing in parsedDirectories)
                {
                    existing.RemoveSuperseded(item);
                }
                parsedDirectories.Add(item);
            }

            return new ParsedConversationDirectories(parsedDirectories);
        }

        private async Task<ParsedConversationDirectory> CreateAsync(ConversationExportDirectory conversationExportDirectory)
        {
            var parsedFiles = new List<ParsedConversationFile>();
            foreach (var file in conversationExportDirectory.ConversationFiles)
            {
                var parseResult = await conversationsParser.GetConversationsAsync(file);
                parsedFiles.Add(new ParsedConversationFile
                {
                    File = file,
                    Conversations = parseResult.Conversations,
                    ParseStatus = parseResult.Status,
                });
            }

            return new ParsedConversationDirectory
            {
                DirectoryInfo = conversationExportDirectory.DirectoryInfo,
                ParsedConversationFiles = parsedFiles,
            };
        }
    }
}
