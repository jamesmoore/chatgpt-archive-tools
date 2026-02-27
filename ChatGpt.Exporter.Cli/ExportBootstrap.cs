using ChatGpt.Exporter.Cli.Assets;
using ChatGPTExport;
using ChatGPTExport.Assets;
using ChatGPTExport.Decoders.AssetRenderer;
using ChatGPTExport.Visitor;
using System.IO.Abstractions;

namespace ChatGpt.Exporter.Cli
{
    internal class ExportBootstrap(
        ParsedConversationDirectoryFactory factory,
        ExportAssetLocatorFactory exportAssetLocatorFactory,
        ConversationExporter exporter,
        IEnumerable<ExportType> exportTypes,
        bool showHidden)
    {
        public int RunExport(IEnumerable<ConversationExportDirectory> conversationExportDirectories, IDirectoryInfo destination)
        {
            var fileConversationsMap = factory.Create(conversationExportDirectories);

            var failedValidation = fileConversationsMap.GetFilesWithStatus(ConversationParseStatus.ValidationFail).ToList();
            if (failedValidation.Count != 0)
            {
                foreach (var conversationFile in failedValidation)
                {
                    Console.Error.WriteLine("Invalid conversation json in " + conversationFile.File.FullName);
                }
                return 1;
            }

            var failedToParse = fileConversationsMap.GetFilesWithStatus(ConversationParseStatus.Error).ToList();
            if (failedToParse.Count != 0)
            {
                Console.Error.WriteLine($"Failed to parse {failedToParse.Count} file(s) due to errors:");
                foreach (var conversationFile in failedToParse)
                {
                    Console.Error.WriteLine($"  - {conversationFile.File.FullName}");
                }
            }

            var successfulConversations = fileConversationsMap.GetFilesWithStatus(ConversationParseStatus.Success)
                .Select(p => (Conversations: p.Conversations!, ConversationAssets: ConversationAssets.FromConversationsFile(p.File)))
                .ToList();

            var conversations = fileConversationsMap.GetLatestConversations();

            var conversationAssetsList = successfulConversations.OrderByDescending(p => p.Conversations.GetUpdateTime()).Select(p => p.ConversationAssets);
            var assetLocator = exportAssetLocatorFactory.GetAssetLocator(conversationAssetsList);
            var assetRenderer = new RelativePathMarkdownAssetRenderer();

            var markdownContentVisitor = new MarkdownContentVisitor(assetLocator, assetRenderer);

            var formatters = new ConversationFormatterFactory().GetFormatters(exportTypes, markdownContentVisitor);

            var count = conversations.Count();
            var position = 0;
            foreach (var conversation in conversations)
            {
                var percent = (int)(position++ * 100.0 / count);
                ConsoleFeatures.SetProgress(percent);
                exporter.Process(conversation, formatters, destination, showHidden);
            }

            return 0;
        }
    }
}
