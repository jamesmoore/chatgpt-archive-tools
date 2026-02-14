using ChatGPTExport;
using ChatGPTExport.Assets;
using System.IO.Abstractions;

namespace ChatGpt.Archive.Api.Services
{
    public class ConversationAssetsCache : IConversationAssetsCache
    {
        private Lazy<IList<ConversationAssets>> conversationAssets;
        private readonly ArchiveSourcesOptions options;
        private readonly ConversationFinder conversationFinder;
        private readonly IFileSystem fileSystem;

        public ConversationAssetsCache(
            ArchiveSourcesOptions options,
            ConversationFinder conversationFinder,
            IFileSystem fileSystem
        )
        {
            this.options = options;
            this.conversationFinder = conversationFinder;
            this.fileSystem = fileSystem;
            this.conversationAssets = new Lazy<IList<ConversationAssets>>(() => GetConversationAssets().ToList());
        }

        private IEnumerable<ConversationAssets> GetConversationAssets()
        {
            var sourceDirectories = options.SourceDirectories.Select(p => fileSystem.DirectoryInfo.New(p));
            var conversationAssets = conversationFinder.GetConversationFiles(sourceDirectories).OrderByDescending(p => p.LastWriteTimeUtc);
            var assetsDirectries = conversationAssets.Select(p => p.Directory).Select(p => ConversationAssets.FromDirectory(p!));
            return assetsDirectries;
        }

        public MediaAssetDefinition? FindMediaAsset(string searchPattern)
        {
            if (conversationAssets == null)
            {
                return null;
            }

            var foundAsset = conversationAssets.Value.Select((p, i) =>
            (
                Index: i,
                p.ParentDirectory,
                Asset: p.FindAsset(searchPattern)
            )).FirstOrDefault(p => p.Asset != null);

            if (foundAsset.Asset == null)
            {
                return null;
            }

            var asset = foundAsset.Asset!;

            return new MediaAssetDefinition(asset.Name, foundAsset.Index, asset.GetRelativePathTo(foundAsset.ParentDirectory));
        }

        public string? GetMediaAssetPath(int index, string relativePath)
        {
            var conversationAssets = this.conversationAssets?.Value;
            if (conversationAssets == null || index >= conversationAssets.Count)
            {
                return null;
            }

            var parentPath = conversationAssets[index].ParentDirectory;
            return parentPath.FileSystem.Path.Combine(parentPath.FullName, relativePath);
        }

        public void Reset()
        {
            conversationAssets = new Lazy<IList<ConversationAssets>>(() => GetConversationAssets().ToList());
        }
    }
}
