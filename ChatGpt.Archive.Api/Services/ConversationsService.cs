using ChatGPTExport;
using ChatGPTExport.Assets;
using ChatGPTExport.Models;
using System.IO.Abstractions;

namespace ChatGpt.Archive.Api.Services
{
    public class ConversationsService : IConversationsService
    {
        private readonly ArchiveSourcesOptions _options;
        private readonly IFileSystem _fileSystem;
        private readonly IConversationAssetsCache _directoryCache;
        private readonly IArchiveRepository _archiveRepository;
        private bool _databaseInitialized = false;
        private readonly object _initLock = new object();

        public ConversationsService(
            IFileSystem fileSystem,
            IConversationAssetsCache directoryCache,
            IArchiveRepository archiveRepository,
            ArchiveSourcesOptions options)
        {
            _fileSystem = fileSystem;
            _directoryCache = directoryCache;
            _archiveRepository = archiveRepository;
            _options = options;
        }

        private void EnsureDatabaseInitialized()
        {
            if (_databaseInitialized)
                return;

            lock (_initLock)
            {
                if (_databaseInitialized)
                    return;

                // Ensure schema exists
                _archiveRepository.EnsureSchema();

                // Check if we need to import conversations
                if (!_archiveRepository.HasConversations())
                {
                    // Import conversations from source
                    var conversations = GetConversationsFromSource();
                    _archiveRepository.InsertConversations(conversations);
                }

                _databaseInitialized = true;
            }
        }

        public IEnumerable<Conversation> GetLatestConversations()
        {
            EnsureDatabaseInitialized();
            return _archiveRepository.GetAll();
        }

        private IEnumerable<Conversation> GetConversationsFromSource()
        {
            var directories = _options.SourceDirectories.Select(p => _fileSystem.DirectoryInfo.New(p));
            var conversationFinder = new ConversationFinder();
            var conversationFiles = conversationFinder.GetConversationFiles(directories);
            var conversationsParser = new ConversationsParser([]);
            var conversations = conversationFiles.Select(p => new { 
                ParsedConversations = conversationsParser.GetConversations(p), 
                ParentDirectory = p.Directory
            }).ToList();
            var successfulConversations = conversations.Where(p => p.ParsedConversations.Status == ConversationParseResult.Success && p.ParsedConversations.Conversations != null).ToList();

            var parentDirectories = successfulConversations.OrderByDescending(p => p.ParsedConversations.Conversations!.GetUpdateTime()).Select(p => p.ParentDirectory!).ToList();
            _directoryCache.SetConversationAssets(parentDirectories.Select(ConversationAssets.FromDirectory).ToList());
            var latestConversations = successfulConversations.Select(p => p.ParsedConversations.Conversations!).GetLatestConversations();
            return latestConversations;
        }

        public Conversation? GetConversation(string conversationId)
        {
            EnsureDatabaseInitialized();
            return _archiveRepository.GetById(conversationId);
        }
    }
}
