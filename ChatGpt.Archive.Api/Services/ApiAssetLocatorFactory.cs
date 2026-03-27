using ChatGPTExport;
using ChatGPTExport.Assets;
using System.IO.Abstractions;

namespace ChatGpt.Archive.Api.Services
{
    public class ApiAssetLocatorFactory(
        ArchiveSourcesOptions options,
        IFileSystem fileSystem,
        ConversationFinder conversationFinder,
        CompositeAssetLocatorFactory compositeAssetLocatorFactory) : IApiAssetLocatorFactory
    {
        public IFileSystemAssetLocator Create()
        {
            var sourceDirectories = options.SourceDirectories.Select(fileSystem.DirectoryInfo.New);

            // Compared to the CLI, this uses a slightly different approach - per file here, ordered by last write time.
            // CLI orders by parsed conversation update time (more expensive).
            var conversationFiles = conversationFinder.GetConversationFiles(sourceDirectories).SelectMany(p => p.ConversationFiles).OrderByDescending(p => p.LastWriteTimeUtc);
            var grouped = conversationFiles.GroupBy(p => p.Directory?.FullName).ToList();
            var firstFileFromEachGroup = grouped.Select(p => p.First());
            var conversationAssets = firstFileFromEachGroup.Select(ConversationAssets.FromConversationsFile);
            var compositeAssetLocator = compositeAssetLocatorFactory.GetAssetLocator(conversationAssets);
            return compositeAssetLocator;
        }
    }
}
