using ChatGPTExport;
using ChatGPTExport.Formatters;
using ChatGPTExport.Models;
using System.Buffers;
using System.IO.Abstractions;
using System.Text;

namespace ChatGpt.Exporter.Cli
{
    public class ConversationExporter(IFileSystem fileSystem, ExportMode exportMode)
    {
        /// <summary>
        /// Processes an instance of a conversation.
        /// </summary>
        /// <param name="conversation">Conversation.</param>
        /// <param name="formatters">The conversation formatters.</param>
        /// <param name="destination">Destination directory.</param>
        /// <param name="showHidden">Whether to show hidden messages.</param>
        /// <exception cref="ApplicationException"></exception>
        public void Process(
            Conversation conversation,
            IEnumerable<IConversationFormatter> formatters,
            IDirectoryInfo destination,
            bool showHidden)
        {
            try
            {
                var fileContentsMap = new Dictionary<string, FormattedConversation>();

                Console.WriteLine(conversation.title);

                Console.WriteLine($"\tMessages: {conversation.mapping!.Count}\tLeaves: {conversation.mapping.Count(p => p.Value.IsLeaf())}");

                var conversationToExport = exportMode == ExportMode.Complete ? conversation : conversation.GetLastestConversation();
                foreach (var formatter in formatters)
                {
                    Console.Write($"\t\t{formatter.GetType().Name}");
                    var exportFilename = GetFilename(conversationToExport, "");
                    ExportConversation(fileContentsMap, formatter, conversationToExport, exportFilename, showHidden);
                    Console.WriteLine($"...Done");
                }

                foreach (var (filename, formattedConversation) in fileContentsMap)
                {
                    var destinationFilename = fileSystem.Path.Join(destination.FullName, filename);
                    var contents = formattedConversation.Contents;
                    var destinationExists = fileSystem.File.Exists(destinationFilename);
                    if (destinationExists == false || destinationExists && FileStringMismatch(destinationFilename, contents))
                    {
                        fileSystem.File.WriteAllText(destinationFilename, contents);
                        fileSystem.File.SetCreationTimeUtcIfPossible(destinationFilename, conversation.GetCreateTime().DateTime);
                        fileSystem.File.SetLastWriteTimeUtc(destinationFilename, conversation.GetUpdateTime().DateTime);
                        Console.WriteLine($"\t{filename}...Saved");
                    }
                    else
                    {
                        Console.WriteLine($"\t{filename}...No change");
                    }

                    foreach (var asset in formattedConversation.AllAssets)
                    {
                        Console.Write($"\t\t{asset.Name}...");
                        // previously for filesystem assets:
                        // Console.Write($"\t\t{string.Join("/", markdownAsset.PathSegments)}...");
                        var result = asset.SaveToFileSystem(destination);
                        Console.WriteLine(result);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
            }
        }

        private bool FileStringMismatch(string destinationFilename, string contents)
        {
            var encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
            var fileInfo = fileSystem.FileInfo.New(destinationFilename);
            if (fileInfo.Length != encoding.GetByteCount(contents))
            {
                return true;
            }

            using var stream = fileSystem.File.OpenRead(destinationFilename);
            using var reader = new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks: true, bufferSize: 4096);
            var buffer = ArrayPool<char>.Shared.Rent(4096);
            try
            {
                var offset = 0;
                int read;
                while ((read = reader.Read(buffer, 0, buffer.Length)) > 0)
                {
                    if (offset + read > contents.Length)
                    {
                        return true;
                    }

                    if (!contents.AsSpan(offset, read).SequenceEqual(buffer.AsSpan(0, read)))
                    {
                        return true;
                    }

                    offset += read;
                }

                return offset != contents.Length;
            }
            finally
            {
                ArrayPool<char>.Shared.Return(buffer);
            }
        }

        private static void ExportConversation(
            Dictionary<string, FormattedConversation> fileContentsMap,
            IConversationFormatter formatter,
            Conversation conversation,
            string filename,
            bool showHidden)
        {
            try
            {
                var formattedConversation = formatter.Format(conversation, ".", showHidden);
                fileContentsMap[filename + formattedConversation.Extension] = formattedConversation;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }

        private string GetFilename(Conversation conversation, string modifier)
        {
            var createtime = conversation.GetCreateTime();
            var value = $"{createtime:yyyy-MM-ddTHH-mm-ss} - {conversation.title}{(string.IsNullOrWhiteSpace(modifier) ? "" : $" - {modifier}")}";
            value = new string(value.Where(p => fileSystem.Path.GetInvalidFileNameChars().Contains(p) == false).ToArray());
            return value.Trim();
        }
    }
}

