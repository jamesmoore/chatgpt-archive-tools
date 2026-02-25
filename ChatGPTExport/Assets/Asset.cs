using System.IO;
using System.IO.Abstractions;

namespace ChatGPTExport.Assets
{
    public record Asset(
        string Name, 
        string MarkdownPath,
        IFileInfo FileInfo,
        string[] PathSegments,
        DateTimeOffset? CreatedDate,
        DateTimeOffset? UpdatedDate
        )
    {
        public Stream GetStream() => FileInfo.OpenRead();
    }
}