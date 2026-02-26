using ChatGPTExport.Assets;

namespace ChatGPTExport.Decoders
{
    public record MarkdownContentResult(IEnumerable<string> Lines, IEnumerable<Asset> Assets, string? Suffix = null, bool HasImage = false)
    {
        public static MarkdownContentResult Empty() 
            => new([], []);

        public static MarkdownContentResult FromLine(string line, string? suffix = null) 
            => new([line], [], suffix);

        public static MarkdownContentResult FromLines(IEnumerable<string> lines) 
            => new(lines, []);

        public static MarkdownContentResult FromLinesWithSuffix(IEnumerable<string> lines, string suffix) 
            => new(lines, [], suffix);
    }
}
