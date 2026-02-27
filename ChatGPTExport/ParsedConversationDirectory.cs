using ChatGPTExport.Models;
using System.IO.Abstractions;

namespace ChatGPTExport
{
    public class ParsedConversationDirectories(IEnumerable<ParsedConversationDirectory> parsedConversationDirectories)
    {
        public IEnumerable<Conversation> GetLatestConversations()
        {          
            return parsedConversationDirectories.Select(p => p.GetConsolidatedConversations()).Where(p => p != null).Select(p => p!).GetLatestConversations();
        }

        public IEnumerable<ParsedConversationFile> GetFilesWithStatus(ConversationParseStatus status)
        {
            return parsedConversationDirectories.SelectMany(p => p.GetFilesWithStatus(status));
        }

        /// <summary>
        /// When obtaining assets we want to prioritize the most recently updated conversations.
        /// Returns the most recently conversation file per directory, in recently updated order.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ParsedConversationFile> GetMostRecentlyUpdatedConversationsFilesPerDirectory()
        {
            var mostRecent = parsedConversationDirectories
                .Select(p => p.GetMostRecentlyUpdatedConversationsFile())
                .Where(p => p != null)
                .Select(p => p!)
                .OrderByDescending(p => p!.Conversations!.GetUpdateTime())
                .ToList();
            return mostRecent;
        }
    }

    public class ParsedConversationDirectory
    {
        public required IDirectoryInfo DirectoryInfo { get; set; }
        public required IEnumerable<ParsedConversationFile> ParsedConversationFiles { get; set; }

        public IEnumerable<ParsedConversationFile> GetFilesWithStatus(ConversationParseStatus status)
        {
            return ParsedConversationFiles.Where(f => f.ParseStatus == status);
        }

        public ParsedConversationFile? GetMostRecentlyUpdatedConversationsFile()
        {
            var successfulFiles = GetFilesWithStatus(ConversationParseStatus.Success).OrderByDescending(p => p.Conversations!.GetUpdateTime());
            return successfulFiles.FirstOrDefault();
        }

        /// <summary>
        /// Consolidate conversations at the directory level to handle multi-file conversations.
        /// </summary>
        /// <returns></returns>
        public Conversations? GetConsolidatedConversations()
        {
            var successfulFiles = GetFilesWithStatus(ConversationParseStatus.Success)
                .Select(p => p.Conversations!).
                GetLatestConversations();

            var newConversations = new Conversations();
            newConversations.AddRange(successfulFiles);
            return newConversations;
        }
    }

    public class ParsedConversationFile
    {
        public required IFileInfo File { get; set; }
        public required Conversations? Conversations { get; set; }
        public required ConversationParseStatus ParseStatus { get; set; }
    }
}
