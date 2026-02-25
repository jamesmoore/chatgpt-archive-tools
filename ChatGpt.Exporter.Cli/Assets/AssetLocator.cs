using ChatGPTExport.Assets;
using System.IO.Abstractions;

namespace ChatGpt.Exporter.Cli.Assets
{
    public class AssetLocator(
        ConversationAssets conversationAssets
        ) : IAssetLocator
    {
        private static readonly HashSet<string> AllowedRoles = new(StringComparer.OrdinalIgnoreCase)
        {
            "system",
            "user",
            "assistant",
            "tool",
            "function"
        };

        public Asset? GetMarkdownMediaAsset(AssetRequest assetRequest)
        {
            var sourceFile = conversationAssets.FindAsset(assetRequest.SearchPattern);
            if (sourceFile != null)
            {
                var sanitizedRole = SanitizeRole(assetRequest.Role);
                var destinationAssetsPath = $"{sanitizedRole}-assets";
                var escapedAssetPath = Uri.EscapeDataString(sourceFile.Name);
                return new Asset(
                    sourceFile.Name,
                    $"./{destinationAssetsPath}/{escapedAssetPath}",
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
