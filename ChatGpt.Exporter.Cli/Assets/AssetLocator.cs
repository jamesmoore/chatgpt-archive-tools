using ChatGPTExport.Assets;
using System.IO.Abstractions;

namespace ChatGpt.Exporter.Cli.Assets
{
    public class AssetLocator(
        ConversationAssets sourceDirectory
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
            var sourceFile = sourceDirectory.FindAsset(assetRequest.SearchPattern);
            if (sourceFile != null)
            {
                var assetWithoutPath = sourceFile.Name;
                var sanitizedRole = SanitizeRole(assetRequest.Role);
                var destinationAssetsPath = $"{sanitizedRole}-assets";
                //var destinationAssetsDir = fileSystem.Path.Join(destinationDirectory.FullName, destinationAssetsPath);
                //if (fileSystem.Directory.Exists(destinationAssetsDir) == false)
                //{
                //    fileSystem.Directory.CreateDirectory(destinationAssetsDir);
                //}

                //var fullDestinationAssetPath = fileSystem.Path.Combine(destinationAssetsDir, assetWithoutPath);

                //if (fileSystem.File.Exists(fullDestinationAssetPath) == false)
                //{
                //    fileSystem.File.Copy(sourceFile.FullName, fullDestinationAssetPath, true);
                //    existingAssetLocator.Add(fullDestinationAssetPath);

                //    if (assetRequest.CreatedDate.HasValue)
                //    {
                //        fileSystem.File.SetCreationTimeUtcIfPossible(fullDestinationAssetPath, assetRequest.CreatedDate.Value.DateTime);
                //    }

                //    if (assetRequest.UpdatedDate.HasValue)
                //    {
                //        fileSystem.File.SetLastWriteTimeUtc(fullDestinationAssetPath, assetRequest.UpdatedDate.Value.DateTime);
                //    }
                //}

                string escapedAssetPath = Uri.EscapeDataString(assetWithoutPath);
                return new Asset(
                    assetWithoutPath,
                    $"./{destinationAssetsPath}/{escapedAssetPath}",
                    sourceFile,
                    [destinationAssetsPath, assetWithoutPath]
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
