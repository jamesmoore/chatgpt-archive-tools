using System.IO.Abstractions;

namespace ChatGPTExport.Assets
{
    public record Asset(
        string Name, 
        string RelativePath,
        IFileInfo FileInfo
        )
    {
        public string GetMarkdownLink() => $"![{Name}]({RelativePath})  ";
    }
}