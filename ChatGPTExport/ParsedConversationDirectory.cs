using ChatGPTExport.Models;
using System.IO.Abstractions;

namespace ChatGPTExport
{
    public class ParsedConversationDirectories(IEnumerable<ParsedConversationDirectory> parsedConversationDirectories)
    {
        public IEnumerable<Conversation> GetLatestConversations()
        {          
            return parsedConversationDirectories
                .Select(p => p.GetConsolidatedConversations())
                .Where(p => p != null)
                .Select(p => p!)
                .GetLatestConversations()
                .ToList();
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

        internal ParsedConversationFile? GetMostRecentlyUpdatedConversationsFile()
        {
            var successfulFiles = GetFilesWithStatus(ConversationParseStatus.Success).OrderByDescending(p => p.Conversations!.GetUpdateTime());
            return successfulFiles.FirstOrDefault();
        }

        /// <summary>
        /// Consolidate conversations at the directory level to handle multi-file conversations.
        /// </summary>
        /// <returns></returns>
        internal Conversations? GetConsolidatedConversations()
        {
            var successfulFiles = GetFilesWithStatus(ConversationParseStatus.Success)
                .Select(p => p.Conversations!).
                GetLatestConversations();

            var newConversations = new Conversations();
            newConversations.AddRange(successfulFiles);
            return newConversations;
        }

        internal void RemoveSuperseded(ParsedConversationDirectory item)
        {
            foreach (var file in item.GetFilesWithStatus(ConversationParseStatus.Success).ToList())
            {
                foreach(var existing in this.GetFilesWithStatus(ConversationParseStatus.Success).ToList())
                {
                    existing.RemoveSuperseded(file);
                }
            }
        }
    }

    public class ParsedConversationFile
    {
        public required IFileInfo File { get; set; }
        public required Conversations? Conversations { get; set; }
        public required ConversationParseStatus ParseStatus { get; set; }

        internal void RemoveSuperseded(ParsedConversationFile newItem)
        {
            if (this.Conversations == null || newItem.Conversations == null || this.Conversations.Count == 0 || newItem.Conversations.Count == 0)
            {
                return;
            }

            var latestInThis = new Dictionary<string, decimal>(StringComparer.Ordinal);
            foreach (var conversation in this.Conversations.Where(p => p.conversation_id != null))
            {
                if (!latestInThis.TryGetValue(conversation.conversation_id!, out var latest) || conversation.update_time > latest)
                {
                    latestInThis[conversation.conversation_id!] = conversation.update_time;
                }
            }

            var latestInNewItem = new Dictionary<string, decimal>(StringComparer.Ordinal);
            foreach (var conversation in newItem.Conversations.Where(p => p.conversation_id != null))
            {
                if (!latestInNewItem.TryGetValue(conversation.conversation_id!, out var latest) || conversation.update_time > latest)
                {
                    latestInNewItem[conversation.conversation_id!] = conversation.update_time;
                }
            }

            this.Conversations.RemoveAll(conversation =>
                conversation.conversation_id != null &&
                latestInNewItem.TryGetValue(conversation.conversation_id, out var latestInOther) &&
                conversation.update_time <= latestInOther);

            newItem.Conversations.RemoveAll(conversation =>
                conversation.conversation_id != null &&
                latestInThis.TryGetValue(conversation.conversation_id, out var latestInOther) &&
                conversation.update_time < latestInOther);
        }
    }
}
