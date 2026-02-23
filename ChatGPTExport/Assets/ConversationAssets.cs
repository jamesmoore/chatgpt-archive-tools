using System.IO;
using System.IO.Abstractions;

namespace ChatGPTExport.Assets
{
    /// <summary>
    /// Represents the assets for a given conversations file.
    /// </summary>
    public class ConversationAssets
    {
        public static ConversationAssets FromConversationsFile(IFileInfo conversationsFile)
        {
            if (conversationsFile.Name != "conversations.json" || conversationsFile.Exists == false)
            {
                throw new ArgumentException("The provided file must be named 'conversations.json' and must exist.", nameof(conversationsFile));

            }
            if (conversationsFile.Directory == null)
            {
                throw new ArgumentException("The provided file must have a valid parent directory.", nameof(conversationsFile));
            }
            return new ConversationAssets(conversationsFile.Directory);
        }

        private readonly IDirectoryInfo parentDirectory;
        private readonly Lazy<string[]> cachedFiles;

        public IDirectoryInfo ParentDirectory => parentDirectory;

        private ConversationAssets(IDirectoryInfo parentDirectory)
        {
            this.parentDirectory = parentDirectory;
            // Use Lazy<T> for thread-safe lazy initialization
            this.cachedFiles = new Lazy<string[]>(() =>
                parentDirectory.FileSystem.Directory
                    .EnumerateFiles(parentDirectory.FullName, "*", SearchOption.AllDirectories)
                    .ToArray()
            );
        }

        public IFileInfo? FindAsset(string searchPattern)
        {
            // Lazy initialization: only enumerate files on first access
            var path = cachedFiles.Value.FirstOrDefault(p => p.Contains(searchPattern));
            return path == null ? null : parentDirectory.FileSystem.FileInfo.New(path);
        }
    }
}
