using System.IO.Abstractions;

namespace ChatGPTExport
{
    public static class FileSystemExtensions
    {
        public static string GetRelativePathTo(this IFileInfo targetFile, IDirectoryInfo baseDir)
        {
            var fileSystem = targetFile.FileSystem;
            var basePath = fileSystem.Path.GetFullPath(baseDir.FullName + fileSystem.Path.DirectorySeparatorChar);
            var targetPath = targetFile.FullName;

            if (!string.Equals(fileSystem.Path.GetPathRoot(basePath), fileSystem.Path.GetPathRoot(targetPath), StringComparison.OrdinalIgnoreCase))
            {
                // Different drives, return full path
                return targetPath;
            }

            var baseUri = new Uri(basePath);
            var targetUri = new Uri(targetPath);

            var relativePath = Uri.UnescapeDataString(baseUri.MakeRelativeUri(targetUri).ToString())
                                     .Replace('/', fileSystem.Path.DirectorySeparatorChar);
            return relativePath;
        }
    }
}
