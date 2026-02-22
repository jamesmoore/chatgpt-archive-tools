using System.IO.Abstractions;
using ChatGPTExport;

namespace ChatGpt.Exporter.Cli
{
    internal record ExportArgs(
        IEnumerable<IDirectoryInfo> SourceDirectory, 
        IDirectoryInfo DestinationDirectory, 
        ExportMode ExportMode, 
        IEnumerable<ExportType> ExportTypes, 
        bool ShowHidden);
}
