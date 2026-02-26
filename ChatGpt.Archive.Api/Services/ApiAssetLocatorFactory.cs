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
            var sourceDirectories = options.SourceDirectories.Select(fileSystem.DirectoryInfo.New);
            var conversationFiles = conversationFinder.GetConversationFiles(sourceDirectories).SelectMany(p => p.ConversationFiles).OrderByDescending(p => p.LastWriteTimeUtc);
            var conversationAssets = conversationFiles.Select(ConversationAssets.FromConversationsFile);
            var assetLocators = conversationAssets.Select(p => new AssetLocator(p)).ToList();
            var compositeAssetLocator = new CompositeAssetLocator(assetLocators);
            return new ApiAssetLocator(compositeAssetLocator, conversationAssetsCache);
        }
    }
}
