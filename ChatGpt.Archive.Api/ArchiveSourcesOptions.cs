namespace ChatGpt.Archive.Api
{
    public sealed class ArchiveSourcesOptions
    {
        public List<string> SourceDirectories { get; init; } = [];
        public string DataDirectory { get; init; } = string.Empty;
    }
}
