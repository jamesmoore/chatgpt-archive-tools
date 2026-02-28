using System.IO.Abstractions;

namespace ChatGPTExport.Assets
{
    public class FileSystemAssetLocator(
        ConversationAssets conversationAssets
        ) : IFileSystemAssetLocator
    {
        private static readonly HashSet<string> AllowedRoles = new(StringComparer.OrdinalIgnoreCase)
        {
            "system",
            "user",
            "assistant",
            "tool",
            "function"
        };

        public FileSystemAsset? GetMarkdownMediaAsset(FileSystemAssetRequest assetRequest)
        {
            var sourceFile = conversationAssets.FindAsset(assetRequest.SearchPattern);
            if (sourceFile != null)
            {
                var sanitizedRole = SanitizeRole(assetRequest.Role);
                var destinationAssetsPath = $"{sanitizedRole}-assets";
                return new FileSystemAsset(
                    sourceFile.Name,
                    sourceFile,
                    [destinationAssetsPath, sourceFile.Name],
                    assetRequest.CreatedDate,
                    assetRequest.UpdatedDate
                    );
            }

            return null;
        }

        private string SanitizeRole(string role)
        {
            if (AllowedRoles.Contains(role))
            {
                return role;
            }

            // TODO fix up later
            var fileSystem = new FileSystem();

            var invalidChars = fileSystem.Path.GetInvalidFileNameChars()
                .Concat(new[]
                {
                    fileSystem.Path.DirectorySeparatorChar,
                    fileSystem.Path.AltDirectorySeparatorChar,
                    '.'
                })
                .ToHashSet();

            var cleaned = new string(role.Where(c => !invalidChars.Contains(c)).ToArray());

            return AllowedRoles.Contains(cleaned) ? cleaned : "unknown";
        }
    }
}
