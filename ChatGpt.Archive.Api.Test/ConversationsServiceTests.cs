using ChatGpt.Archive.Api.Database;
using ChatGpt.Archive.Api.Services;
using ChatGPTExport;
using ChatGPTExport.Assets;
using ChatGPTExport.Decoders.AssetRenderer;
using ChatGPTExport.Models;
using System.IO.Abstractions.TestingHelpers;

namespace ChatGpt.Archive.Api.Test
{
    public class ConversationsServiceTests
    {
        [Fact]
        public async Task LoadConversationsAsync_RefreshesAssetLocator()
        {
            var factory = new TrackingAssetLocatorFactory();
            var service = CreateService(factory);

            Assert.Equal(1, factory.CreateCallCount);

            await service.LoadConversationsAsync();

            Assert.Equal(2, factory.CreateCallCount);
        }

        [Fact]
        public async Task LoadConversationsAsync_CalledTwice_RefreshesAssetLocatorEachTime()
        {
            var factory = new TrackingAssetLocatorFactory();
            var service = CreateService(factory);

            await service.LoadConversationsAsync();
            await service.LoadConversationsAsync();

            Assert.Equal(3, factory.CreateCallCount);
        }

        private static ConversationsService CreateService(IApiAssetLocatorFactory factory)
        {
            var fileSystem = new MockFileSystem();
            var archiveRepository = new StubArchiveRepository();
            var options = new ArchiveSourcesOptions { SourceDirectories = [], DataDirectory = "/data" };
            var conversationFinder = new ConversationFinder();
            var markdownAssetRenderer = new MarkdownAssetRenderer("/asset");
            var conversationFormatterFactory = new ConversationFormatterFactory();
            var assetsCache = new AssetsCache();

            return new ConversationsService(
                fileSystem,
                archiveRepository,
                conversationFinder,
                options,
                factory,
                markdownAssetRenderer,
                conversationFormatterFactory,
                assetsCache);
        }

        private class TrackingAssetLocatorFactory : IApiAssetLocatorFactory
        {
            public int CreateCallCount { get; private set; }

            public IFileSystemAssetLocator Create()
            {
                CreateCallCount++;
                return new NullAssetLocator();
            }
        }

        private class NullAssetLocator : IFileSystemAssetLocator
        {
            public FileSystemAsset? GetMarkdownMediaAsset(FileSystemAssetRequest assetRequest) => null;
        }

        private class StubArchiveRepository : IArchiveRepository
        {
            public bool HasConversations() => false;
            public void ClearAll() { }
            public void InsertConversations(IEnumerable<Conversation> conversations) { }
            public IEnumerable<Conversation> GetAll() => [];
            public Conversation? GetById(string id) => null;
            public IEnumerable<SearchResult> Search(string query) => [];
        }
    }
}
