using ChatGPTExport;
using ChatGPTExport.Assets;
using System.IO.Abstractions;

namespace ChatGpt.Exporter.Cli.Assets
{
    public class ExistingAssetLocator(IDirectoryInfo destinationDirectory) : IAssetLocator
    {
        private static readonly char[] InvalidPatternChars = Path.GetInvalidFileNameChars()
            .Concat(['*', '?'])
            .Distinct()
            .ToArray();

        private List<string>? cache = null;
        private readonly IFileSystem fileSystem = destinationDirectory.FileSystem;
        private readonly char[] pathSeparators = [destinationDirectory.FileSystem.Path.DirectorySeparatorChar, destinationDirectory.FileSystem.Path.AltDirectorySeparatorChar];

        private IEnumerable<string> GetCachedDestinationFiles(string searchPattern)
        {
            var match = GetCache().Where(p => p.Contains(searchPattern));
            return match;
        }

        private List<string> GetCache()
        {
            cache ??= fileSystem.Directory.GetFiles(destinationDirectory.FullName, "*", System.IO.SearchOption.AllDirectories).ToList();
            return cache;
        }

        public void Add(string newFile)
        {
            GetCache().Add(newFile);
        }

        public Asset? GetMarkdownMediaAsset(AssetRequest assetRequest)
        {
            // Validate search pattern is not null or empty
            if (string.IsNullOrWhiteSpace(assetRequest.SearchPattern))
            {
                return null;
            }

            // Check for invalid characters, wildcards, and path separators
            if (assetRequest.SearchPattern.IndexOfAny(InvalidPatternChars) >= 0 ||
                assetRequest.SearchPattern.IndexOfAny(pathSeparators) >= 0)
            {
                return null;
            }

            // it may already exist in the destination directory from a previous export
            var destinationMatches = GetCachedDestinationFiles(assetRequest.SearchPattern).ToList();
            if (destinationMatches.Count == 0)
            {
                destinationMatches = fileSystem.Directory.GetFiles(destinationDirectory.FullName, assetRequest.SearchPattern + "*.*", System.IO.SearchOption.AllDirectories).ToList();
            }

            if (destinationMatches.Count != 0)
            {
                var targetFile = fileSystem.FileInfo.New(destinationMatches.First());
                var relativePath = targetFile.GetRelativePathTo(destinationDirectory);
                if (fileSystem.Path.DirectorySeparatorChar != '/')
                {
                    relativePath = relativePath.Replace(fileSystem.Path.DirectorySeparatorChar, '/');
                }
                return new Asset(targetFile.Name, $"./{Uri.EscapeDataString(relativePath).Replace("%2F", "/")}");
            }
            return null;
        }
    }
}
