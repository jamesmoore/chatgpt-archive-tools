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
        )
    {
        public Stream GetStream() => FileInfo.OpenRead();
    }
}