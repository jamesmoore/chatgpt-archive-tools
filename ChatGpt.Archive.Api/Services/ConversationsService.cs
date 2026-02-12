using ChatGPTExport;
using ChatGPTExport.Assets;
using ChatGPTExport.Models;
using System.IO.Abstractions;

namespace ChatGpt.Archive.Api.Services
{
    public class ConversationsService(
        IFileSystem fileSystem,
        IConversationAssetsCache directoryCache,
        IArchiveRepository archiveRepository,
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
            var conversationFinder = new ConversationFinder();
            var conversationFiles = conversationFinder.GetConversationFiles(directories);
            var conversationsParser = new ConversationsParser([]);
            var conversations = conversationFiles.Select(p => new { 
                ParsedConversations = conversationsParser.GetConversations(p), 
                ParentDirectory = p.Directory
            }).ToList();
            var successfulConversations = conversations.Where(p => p.ParsedConversations.Status == ConversationParseResult.Success && p.ParsedConversations.Conversations != null).ToList();

            var parentDirectories = successfulConversations.OrderByDescending(p => p.ParsedConversations.Conversations!.GetUpdateTime()).Select(p => p.ParentDirectory!).ToList();
            directoryCache.SetConversationAssets(parentDirectories.Select(ConversationAssets.FromDirectory).ToList());
            var latestConversations = successfulConversations.Select(p => p.ParsedConversations.Conversations!).GetLatestConversations();
            return latestConversations;
        }

        public Conversation? GetConversation(string conversationId)
        {
            EnsureDatabaseInitialized();
            return archiveRepository.GetById(conversationId);
        }
    }
}
