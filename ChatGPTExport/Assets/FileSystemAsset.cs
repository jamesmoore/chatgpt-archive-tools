using ChatGPTExport.Formatters;
using System.IO;
using System.IO.Abstractions;

namespace ChatGPTExport.Assets
{
    public record FileSystemAsset(
        string Name, 
        IFileInfo FileInfo,
        string[] PathSegments,
        DateTimeOffset? CreatedDate,
        DateTimeOffset? UpdatedDate
        ) : IFormattedConversationAsset
    {
        public string MimeType => throw new NotImplementedException();

        public Stream GetStream() => FileInfo.OpenRead();

        public SaveStatus SaveToFileSystem(IDirectoryInfo destination)
        {
            var fileSystem = destination.FileSystem;
            var destinationSegments = new[] { destination.FullName }
                .Concat(this.PathSegments)
                .ToArray();
            var fullDestinationPath = fileSystem.Path.Join(destinationSegments);
            var exists = fileSystem.File.Exists(fullDestinationPath);
            if (exists == false)
            {
                using var stream = this.GetStream();
                fileSystem.SaveToFilesystem(stream, fullDestinationPath, this.CreatedDate, this.UpdatedDate);
                return SaveStatus.Success;
            }
            else
            {
                return SaveStatus.Exists;
            }
        }
    }
}