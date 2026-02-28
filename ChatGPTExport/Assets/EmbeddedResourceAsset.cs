using ChatGPTExport.Formatters;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Abstractions;
using System.Reflection;
using System.Security.Cryptography;

namespace ChatGPTExport.Assets
{
    public class EmbeddedResourceAsset(string name, string resourceName, string mimeType) : IFormattedConversationAsset
    {
        private static readonly Assembly Assembly = typeof(EmbeddedResourceAsset).Assembly;
        private static readonly ConcurrentDictionary<string, string> HashedNameCache = [];

        public string Name { get; } = GetOrComputeHashedName(name, resourceName);

        public string MimeType { get; } = mimeType;

        public Stream GetStream()
        {
            return Assembly.GetManifestResourceStream(resourceName) ??
                throw new InvalidOperationException("Stream unavailable");
        }

        private static string GetOrComputeHashedName(string name, string resourceName)
        {
            if (HashedNameCache.TryGetValue(resourceName, out var cached))
            {
                return cached;
            }

            using var stream = Assembly.GetManifestResourceStream(resourceName) ??
                throw new InvalidOperationException("Stream unavailable");
            var hash = SHA256.HashData(stream);
            var hashString = Convert.ToHexString(hash)[..8].ToLowerInvariant();
            var ext = Path.GetExtension(name);
            var nameWithoutExt = name[..^ext.Length];
            var hashedName = $"{nameWithoutExt}.{hashString}{ext}";
            HashedNameCache[resourceName] = hashedName;
            return hashedName;
        }

        public SaveStatus SaveToFileSystem(IDirectoryInfo destination)
        {
            var fileSystem = destination.FileSystem;
            var assetDestinationFilename = fileSystem.Path.Join(destination.FullName, this.Name);
            if (fileSystem.File.Exists(assetDestinationFilename) == false)
            {
                using var stream = this.GetStream();
                fileSystem.SaveToFilesystem(stream, assetDestinationFilename, null, null);
                return SaveStatus.Success;
            }
            else
            {
                return SaveStatus.Exists;
            }
        }
    }
}
