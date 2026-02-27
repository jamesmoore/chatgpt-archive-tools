using ChatGPTExport;
using ChatGPTExport.Models;
using System.IO.Abstractions.TestingHelpers;

namespace ChatGTPExportTests;

public class ParsedConversationDirectoryTests
{
    [Fact]
    public void GetFilesWithStatus_ReturnsOnlyMatchingFiles()
    {
        var directory = CreateDirectory(
            CreateParsedFile("one.json", ConversationParseStatus.Success, [CreateConversation("conv1", 10)]),
            CreateParsedFile("two.json", ConversationParseStatus.Error, null),
            CreateParsedFile("three.json", ConversationParseStatus.Success, [CreateConversation("conv2", 20)]));

        var result = directory.GetFilesWithStatus(ConversationParseStatus.Success).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, file => Assert.Equal(ConversationParseStatus.Success, file.ParseStatus));
    }

    [Fact]
    public void GetMostRecentlyUpdatedConversationsFile_ReturnsMostRecentSuccessfulFile()
    {
        var newest = CreateParsedFile("newest.json", ConversationParseStatus.Success, [CreateConversation("conv2", 300)]);
        var directory = CreateDirectory(
            CreateParsedFile("older.json", ConversationParseStatus.Success, [CreateConversation("conv1", 100)]),
            CreateParsedFile("failed.json", ConversationParseStatus.Error, null),
            newest);

        var result = directory.GetMostRecentlyUpdatedConversationsFile();

        Assert.NotNull(result);
        Assert.Equal("newest.json", result!.File.Name);
    }

    [Fact]
    public void GetConsolidatedConversations_DeduplicatesByConversationIdAndKeepsLatest()
    {
        var directory = CreateDirectory(
            CreateParsedFile("first.json", ConversationParseStatus.Success, [
                CreateConversation("shared", 100, "old"),
                CreateConversation("first-only", 150)]),
            CreateParsedFile("second.json", ConversationParseStatus.Success, [
                CreateConversation("shared", 200, "new"),
                CreateConversation("second-only", 180)]),
            CreateParsedFile("failed.json", ConversationParseStatus.ValidationFail, null));

        var result = directory.GetConsolidatedConversations()!.OrderBy(c => c.conversation_id).ToList();

        Assert.Equal(3, result.Count);
        Assert.Equal("first-only", result[0].conversation_id);
        Assert.Equal("second-only", result[1].conversation_id);
        Assert.Equal("shared", result[2].conversation_id);
        Assert.Equal("new", result[2].title);
        Assert.Equal(200, result[2].update_time);
    }

    private static ParsedConversationDirectory CreateDirectory(params ParsedConversationFile[] files)
    {
        var fs = new MockFileSystem();
        fs.AddDirectory(@"C:\export");

        return new ParsedConversationDirectory
        {
            DirectoryInfo = fs.DirectoryInfo.New(@"C:\export"),
            ParsedConversationFiles = files
        };
    }

    private static ParsedConversationFile CreateParsedFile(string name, ConversationParseStatus status, Conversations? conversations)
    {
        var fs = new MockFileSystem();
        var path = $@"C:\export\{name}";
        fs.AddDirectory(@"C:\export");
        fs.AddFile(path, new MockFileData("[]"));

        return new ParsedConversationFile
        {
            File = fs.FileInfo.New(path),
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

