using ChatGPTExport;
using ChatGPTExport.Assets;
using System.IO.Abstractions;

namespace ChatGpt.Archive.Api.Services
{
    public class ApiAssetLocatorFactory(
        ArchiveSourcesOptions options, 
        IFileSystem fileSystem,
        ConversationFinder conversationFinder,
        IConversationAssetsCache conversationAssetsCache
        )
    {
        public IAssetLocator Create()
        {
            var conversationAssets = GetConversationAssets();
            var assetLocators = conversationAssets.Select(p => new AssetLocator(p));
            var compositeAssetLocator = new CompositeAssetLocator(assetLocators);
            return new ApiAssetLocator( compositeAssetLocator, conversationAssetsCache);
        }

        private IEnumerable<ConversationAssets> GetConversationAssets()
        {
            var sourceDirectories = options.SourceDirectories.Select(fileSystem.DirectoryInfo.New);
            var conversationAssets = conversationFinder.GetConversationFiles(sourceDirectories).OrderByDescending(p => p.LastWriteTimeUtc);
            var assetsDirectries = conversationAssets.Select(ConversationAssets.FromConversationsFile);
            return assetsDirectries;
        }
    }
}
