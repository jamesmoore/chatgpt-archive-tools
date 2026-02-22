using System.IO;
using System.Reflection;
using System.Security.Cryptography;

namespace ChatGPTExport.Formatters
{
    public interface IFormattedConversationAsset
    {
        public string Name { get; }

        public Stream GetStream();

        public string MimeType { get; }
    }

    public class EmbeddedResourceAsset(string name, string resourceName, string mimeType) : IFormattedConversationAsset
    {
        private static readonly Assembly Assembly = typeof(EmbeddedResourceAsset).Assembly;
        private static readonly Dictionary<string, string> HashedNameCache = [];

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
    }
}
