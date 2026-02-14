using ChatGpt.Archive.Api.Database;
using ChatGPTExport;
using ChatGPTExport.Models;
using System.IO.Abstractions;

namespace ChatGpt.Archive.Api.Services
{
    public class ConversationsService(
        IFileSystem fileSystem,
        IArchiveRepository archiveRepository,
        IConversationAssetsCache conversationAssetsCache,
        ConversationFinder conversationFinder,
        ArchiveSourcesOptions options) : IConversationsService
    {
        private bool _databaseInitialized = false;
        private readonly Lock _initLock = new();

        private void EnsureDatabaseInitialized()
        {
            if (_databaseInitialized)
                return;

            lock (_initLock)
            {
                if (_databaseInitialized)
                    return;

                // Ensure schema exists
                archiveRepository.EnsureSchema();

                // Check if we need to import conversations
                if (!archiveRepository.HasConversations())
                {
                    // Import conversations from source
                    var conversations = GetConversationsFromSource();
                    archiveRepository.InsertConversations(conversations);
                }

                _databaseInitialized = true;
            }
        }

        public IEnumerable<Conversation> GetLatestConversations()
        {
            EnsureDatabaseInitialized();
            return archiveRepository.GetAll();
        }

        private IEnumerable<Conversation> GetConversationsFromSource()
        {
            var directories = options.SourceDirectories.Select(p => fileSystem.DirectoryInfo.New(p));
            var conversationFiles = conversationFinder.GetConversationFiles(directories);
            var conversationsParser = new ConversationsParser([]);
            var conversations = conversationFiles.Select(p => new { 
                ParsedConversations = conversationsParser.GetConversations(p), 
                ParentDirectory = p.Directory
            }).ToList();
            var successfulConversations = conversations.Where(p => p.ParsedConversations.Status == ConversationParseResult.Success && p.ParsedConversations.Conversations != null).ToList();
            var latestConversations = successfulConversations.Select(p => p.ParsedConversations.Conversations!).GetLatestConversations();
            return latestConversations;
        }

        public Conversation? GetConversation(string conversationId)
        {
            EnsureDatabaseInitialized();
            return archiveRepository.GetById(conversationId);
        }

        public IEnumerable<ConsolidatedSearchResult> Search(string query)
        {
            EnsureDatabaseInitialized();
            var flatResult = archiveRepository.Search(query);

            var groupedByConversation = flatResult.GroupBy(r => new { r.ConversationId, r.ConversationTitle });

            var consolidatedResults = groupedByConversation.Select(g => new ConsolidatedSearchResult
            {
                ConversationId = g.Key.ConversationId,
                ConversationTitle = g.Key.ConversationTitle,
                Messages = g.Select(r => new ConsolidatedSearchResult.MessageResult
                {
                    MessageId = r.MessageId,
                    Snippet = r.Snippet
                }).ToList()
            });

            return consolidatedResults;
        }

        public void ClearAll()
        {
            lock (_initLock)
            {
                archiveRepository.ClearAll();
                conversationAssetsCache.Reset();
                _databaseInitialized = false;
            }
        }
    }
}
