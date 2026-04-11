using ChatGPTExport.Assets;

namespace ChatGPTExport.Decoders
{
    public record MarkdownContentResult(IEnumerable<MarkdownContentLine> Lines, IEnumerable<FileSystemAsset> Assets, string? Suffix = null)
    {
        public static MarkdownContentResult Empty() 
            => new([], []);

        public static MarkdownContentResult FromLine(MarkdownContentLine line, string? suffix = null) 
            => new([line], [], suffix);

        public static MarkdownContentResult FromLines(IEnumerable<string> lines, string? suffix = null) 
            => new(lines.Select(line => (MarkdownContentLine)line), [], suffix);

        public string ToMarkdown(string lineBreak)
            => string.Join(lineBreak, Lines.Select(p => p.MarkdownContent));
    }

    public record MarkdownContentLine(string MarkdownContent, bool HasImage = false)
    {
        public static implicit operator MarkdownContentLine(string MarkdownContent) => new(MarkdownContent);
    }
}
