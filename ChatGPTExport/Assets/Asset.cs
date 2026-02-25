using System.IO;
using System.IO.Abstractions;

namespace ChatGPTExport.Assets
{
    public record Asset(
        string Name, 
        string MarkdownPath,
        IFileInfo FileInfo,
        string[] DestinationSegments,
        DateTimeOffset? CreatedDate,
        DateTimeOffset? UpdatedDate
        )
    {
        public string GetMarkdownLink() => $"![{Name}]({MarkdownPath})  ";

        public Stream GetStream() => FileInfo.OpenRead();
    }
}