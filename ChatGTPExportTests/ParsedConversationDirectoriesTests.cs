using ChatGPTExport;
using ChatGPTExport.Models;
using System.IO.Abstractions.TestingHelpers;

namespace ChatGTPExportTests;

public class ParsedConversationDirectoriesTests
{
    [Fact]
    public void GetFilesWithStatus_ReturnsMatchingFilesAcrossDirectories()
    {
        var directories = CreateParsedDirectories(
            CreateDirectory(@"C:\export\one",
                CreateParsedFile(@"C:\export\one\one-success.json", ConversationParseStatus.Success, [CreateConversation("one", 10)]),
                CreateParsedFile(@"C:\export\one\one-error.json", ConversationParseStatus.Error, null)),
            CreateDirectory(@"C:\export\two",
                CreateParsedFile(@"C:\export\two\two-success.json", ConversationParseStatus.Success, [CreateConversation("two", 20)]),
                CreateParsedFile(@"C:\export\two\two-validation.json", ConversationParseStatus.ValidationFail, null)));

        var result = directories.GetFilesWithStatus(ConversationParseStatus.Success).ToList();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, p => p.File.Name == "one-success.json");
        Assert.Contains(result, p => p.File.Name == "two-success.json");
    }

    [Fact]
    public void GetMostRecentlyUpdatedConversationsFilesPerDirectory_ReturnsOnePerDirectoryInDescendingUpdateOrder()
    {
        var directories = CreateParsedDirectories(
            CreateDirectory(@"C:\export\one",
                CreateParsedFile(@"C:\export\one\one-old.json", ConversationParseStatus.Success, [CreateConversation("one-old", 100)]),
                CreateParsedFile(@"C:\export\one\one-new.json", ConversationParseStatus.Success, [CreateConversation("one-new", 200)])),
            CreateDirectory(@"C:\export\two",
                CreateParsedFile(@"C:\export\two\two-newest.json", ConversationParseStatus.Success, [CreateConversation("two", 300)]),
                CreateParsedFile(@"C:\export\two\two-fail.json", ConversationParseStatus.Error, null)),
            CreateDirectory(@"C:\export\three",
                CreateParsedFile(@"C:\export\three\three-fail.json", ConversationParseStatus.Error, null)));

        var result = directories.GetMostRecentlyUpdatedConversationsFilesPerDirectory().ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal("two-newest.json", result[0].File.Name);
        Assert.Equal("one-new.json", result[1].File.Name);
    }

    [Fact]
    public void GetLatestConversations_DeduplicatesAcrossDirectoriesAndKeepsLatestConversation()
    {
        var directories = CreateParsedDirectories(
            CreateDirectory(@"C:\export\one",
                CreateParsedFile(@"C:\export\one\one.json", ConversationParseStatus.Success, [
                    CreateConversation("shared", 100, "older"),
                    CreateConversation("only-one", 150)])),
            CreateDirectory(@"C:\export\two",
                CreateParsedFile(@"C:\export\two\two.json", ConversationParseStatus.Success, [
                    CreateConversation("shared", 300, "newer"),
                    CreateConversation("only-two", 200)])),
            CreateDirectory(@"C:\export\three",
                CreateParsedFile(@"C:\export\three\three-fail.json", ConversationParseStatus.ValidationFail, null)));

        var result = directories.GetLatestConversations().OrderBy(c => c.conversation_id).ToList();

        Assert.Equal(3, result.Count);
        Assert.Equal("only-one", result[0].conversation_id);
        Assert.Equal("only-two", result[1].conversation_id);
        Assert.Equal("shared", result[2].conversation_id);
        Assert.Equal("newer", result[2].title);
        Assert.Equal(300, result[2].update_time);
    }

    private static ParsedConversationDirectories CreateParsedDirectories(params ParsedConversationDirectory[] directories)
    {
        return new ParsedConversationDirectories(directories);
    }

    private static ParsedConversationDirectory CreateDirectory(string directoryPath, params ParsedConversationFile[] files)
    {
        var normalizedDirectoryPath = MockUnixSupport.Path(directoryPath);
        var fs = new MockFileSystem();
        fs.AddDirectory(normalizedDirectoryPath);

        return new ParsedConversationDirectory
        {
            DirectoryInfo = fs.DirectoryInfo.New(normalizedDirectoryPath),
            ParsedConversationFiles = files
        };
    }

    private static ParsedConversationFile CreateParsedFile(string filePath, ConversationParseStatus status, Conversations? conversations)
    {
        var normalizedFilePath = MockUnixSupport.Path(filePath);
        var fs = new MockFileSystem();
        fs.AddDirectory(Path.GetDirectoryName(normalizedFilePath)!);
        fs.AddFile(normalizedFilePath, new MockFileData("[]"));

        return new ParsedConversationFile
        {
            File = fs.FileInfo.New(normalizedFilePath),
            ParseStatus = status,
            Conversations = conversations
        };
    }

    private static Conversation CreateConversation(string id, decimal updateTime, string? title = null)
    {
        return new Conversation
        {
            conversation_id = id,
            title = title,
            update_time = updateTime,
            mapping = []
        };
    }
}
