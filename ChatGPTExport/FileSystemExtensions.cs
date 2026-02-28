using System.IO;
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

        public static void SaveToFilesystem(this IFileSystem fileSystem, Stream stream, string destinationFilename, DateTimeOffset? createdDate, DateTimeOffset? updateDate)
        {
            var destinationDirectory = fileSystem.Path.GetDirectoryName(destinationFilename);
            if (string.IsNullOrEmpty(destinationDirectory) == false)
            {
                fileSystem.Directory.CreateDirectory(destinationDirectory);
            }
            using var fileStream = fileSystem.File.Create(destinationFilename);
            stream.CopyTo(fileStream);

            if (createdDate.HasValue)
            {
                fileSystem.File.SetCreationTimeUtcIfPossible(destinationFilename, createdDate.Value.DateTime);
            }
            if (updateDate.HasValue)
            {
                fileSystem.File.SetLastWriteTimeUtc(destinationFilename, updateDate.Value.DateTime);
            }
        }

        public static void SetCreationTimeUtcIfPossible(this IFile target, string filename, DateTime createdDate)
        {
            if (OperatingSystem.IsWindows())
            {
                target.SetCreationTimeUtc(filename, createdDate);
            }
        }
    }
}
