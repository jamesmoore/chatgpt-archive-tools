using ChatGpt.Archive.Api.Database;
using ChatGPTExport;
using ChatGPTExport.Assets;
using ChatGPTExport.Decoders;
using ChatGPTExport.Models;
using ChatGPTExport.Visitor;
using System.IO.Abstractions;

namespace ChatGpt.Archive.Api.Services
{
    public class ConversationsService(
        IFileSystem fileSystem,
        IArchiveRepository archiveRepository,
        IConversationAssetsCache conversationAssetsCache,
        ConversationFinder conversationFinder,
        ArchiveSourcesOptions options,
        IAssetLocator assetLocator,
        IMarkdownAssetRenderer markdownAssetRenderer,
        ConversationFormatterFactory conversationFormatterFactory,
        AssetsCache assetsCache) : IConversationsService
    {
        private readonly Lock _loadLock = new();

        public void LoadConversations()
        {
            lock (_loadLock)
            {
                // Import conversations from source
                var conversations = GetConversationsFromSource();
                archiveRepository.InsertConversations(conversations);
            }
        }

        public IEnumerable<Conversation> GetLatestConversations()
        {
            return archiveRepository.GetAll();
        }

        private IEnumerable<Conversation> GetConversationsFromSource()
        {
            var directories = options.SourceDirectories.Select(fileSystem.DirectoryInfo.New);
            var conversationFiles = conversationFinder.GetConversationFiles(directories);
            var conversationsParser = new ConversationsParser([]);
            var conversations = conversationFiles.Select(conversationsParser.GetConversations).ToList();
            var successfulConversations = conversations.Where(p => p.Status == ConversationParseResult.Success && p.Conversations != null).ToList();
            var latestConversations = successfulConversations.Select(p => p.Conversations!).GetLatestConversations();
            return latestConversations;
        }

        public Conversation? GetConversation(string conversationId)
        {
            return archiveRepository.GetById(conversationId);
        }

        public IEnumerable<ConsolidatedSearchResult> Search(string query)
        {
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

        public string? GetContent(string conversationId, ExportType exportType)
        {
            var markdownContentVisitor = new MarkdownContentVisitor(assetLocator, markdownAssetRenderer, false);
            var formatter = conversationFormatterFactory.GetFormatters([exportType], markdownContentVisitor);
            var conversation = GetConversation(conversationId);
            if (conversation == null)
            {
                return null;
            }
            var formatted = formatter.First().Format(conversation.GetLastestConversation(), string.Empty);

            if (formatted != null)
            {
                var assets = formatted.Assets;
                if (assets != null)
                {
                    foreach (var asset in assets)
                    {
                        assetsCache.Set(asset.Name, asset);
                    }
                }
            }

            return formatted?.Contents;
        }

        public void ClearAll()
        {
            archiveRepository.ClearAll();
            conversationAssetsCache.Reset();
        }
    }
}
