using ChatGPTExport.Assets;
using ChatGPTExport.Models;
using System.Collections;
using System.IO.Abstractions;

namespace ChatGPTExport
{
    public class ParsedConversationDirectories(IEnumerable<ParsedConversationDirectory> parsedConversationDirectories) //: IEnumerable<ParsedConversationDirectory>
    {
        public IEnumerable<Conversation> GetLatestConversations()
        {
            var successfulConversations = this.GetFilesWithStatus(ConversationParseStatus.Success)
                .Select(p => (Conversations: p.Conversations!, ConversationAssets: ConversationAssets.FromConversationsFile(p.File)))
                .ToList();

            var conversations = successfulConversations
                .Select(p => p.Conversations)
                .GetLatestConversations()
                .ToList();
            return conversations;
        }

        public IEnumerable<ParsedConversationFile> GetFilesWithStatus(ConversationParseStatus status)
        {
            return parsedConversationDirectories.SelectMany(p => p.ParsedConversationFiles.Where(f => f.ParseStatus == status));
        }

        //public IEnumerator<ParsedConversationDirectory> GetEnumerator()
        //{
        //    return parsedConversationDirectories.GetEnumerator();
        //}

        //IEnumerator IEnumerable.GetEnumerator()
        //{
        //    return parsedConversationDirectories.GetEnumerator();
        //}
    }

    public class ParsedConversationDirectory
    {
        public required IDirectoryInfo DirectoryInfo { get; set; }
        public required IEnumerable<ParsedConversationFile> ParsedConversationFiles { get; set; }
    }

    public class ParsedConversationFile
    {
        public required IFileInfo File { get; set; }
        public required Conversations? Conversations { get; set; }
        public required ConversationParseStatus ParseStatus { get; set; }
    }
}
