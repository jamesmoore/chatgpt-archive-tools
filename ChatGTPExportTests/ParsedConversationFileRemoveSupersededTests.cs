using ChatGPTExport;
using ChatGPTExport.Models;
using System.IO.Abstractions.TestingHelpers;

namespace ChatGTPExportTests;

public class ParsedConversationFileRemoveSupersededTests
{
    [Fact]
    public void RemoveSuperseded_WhenExistingIsOlderThanNewItem_RemovesFromExisting()
    {
        var existing = CreateParsedFile("existing.json", [CreateConversation("conv1", 100, "old")]);
        var newItem = CreateParsedFile("new.json", [CreateConversation("conv1", 200, "new")]);

        existing.RemoveSuperseded(newItem);

        Assert.Empty(existing.Conversations!);
        Assert.Single(newItem.Conversations!);
        Assert.Equal("new", newItem.Conversations![0].title);
    }

    [Fact]
    public void RemoveSuperseded_WhenExistingIsNewerThanNewItem_RemovesFromNewItem()
    {
        var existing = CreateParsedFile("existing.json", [CreateConversation("conv1", 200, "new")]);
        var newItem = CreateParsedFile("new.json", [CreateConversation("conv1", 100, "old")]);

        existing.RemoveSuperseded(newItem);

        Assert.Single(existing.Conversations!);
        Assert.Equal("new", existing.Conversations![0].title);
        Assert.Empty(newItem.Conversations!);
    }

    [Fact]
    public void RemoveSuperseded_WhenUpdateTimesAreEqual_FavorsNewItem()
    {
        var existing = CreateParsedFile("existing.json", [CreateConversation("conv1", 100, "existing")]);
        var newItem = CreateParsedFile("new.json", [CreateConversation("conv1", 100, "new")]);

        existing.RemoveSuperseded(newItem);

        Assert.Empty(existing.Conversations!);
        Assert.Single(newItem.Conversations!);
    }

    [Fact]
    public void RemoveSuperseded_WhenConversationsAreUnique_NeitherIsRemoved()
    {
        var existing = CreateParsedFile("existing.json", [CreateConversation("conv1", 100)]);
        var newItem = CreateParsedFile("new.json", [CreateConversation("conv2", 200)]);

        existing.RemoveSuperseded(newItem);

        Assert.Single(existing.Conversations!);
        Assert.Equal("conv1", existing.Conversations![0].conversation_id);
        Assert.Single(newItem.Conversations!);
        Assert.Equal("conv2", newItem.Conversations![0].conversation_id);
    }

    [Fact]
    public void RemoveSuperseded_WithMixedConversations_OnlyRemovesSupersededOnes()
    {
        var existing = CreateParsedFile("existing.json", [
            CreateConversation("shared-old", 100, "old"),
            CreateConversation("existing-only", 150)]);
        var newItem = CreateParsedFile("new.json", [
            CreateConversation("shared-old", 200, "new"),
            CreateConversation("new-only", 180)]);

        existing.RemoveSuperseded(newItem);

        Assert.Single(existing.Conversations!);
        Assert.Equal("existing-only", existing.Conversations![0].conversation_id);
        Assert.Equal(2, newItem.Conversations!.Count);
        Assert.Contains(newItem.Conversations, c => c.conversation_id == "shared-old" && c.title == "new");
        Assert.Contains(newItem.Conversations, c => c.conversation_id == "new-only");
    }

    [Fact]
    public void RemoveSuperseded_WhenExistingConversationsIsNull_DoesNotThrow()
    {
        var existing = CreateParsedFile("existing.json", null);
        var newItem = CreateParsedFile("new.json", [CreateConversation("conv1", 100)]);

        var exception = Record.Exception(() => existing.RemoveSuperseded(newItem));

        Assert.Null(exception);
        Assert.Single(newItem.Conversations!);
    }

    [Fact]
    public void RemoveSuperseded_WhenNewItemConversationsIsNull_DoesNotThrow()
    {
        var existing = CreateParsedFile("existing.json", [CreateConversation("conv1", 100)]);
        var newItem = CreateParsedFile("new.json", null);

        var exception = Record.Exception(() => existing.RemoveSuperseded(newItem));

        Assert.Null(exception);
        Assert.Single(existing.Conversations!);
    }

    [Fact]
    public void RemoveSuperseded_WhenExistingConversationsIsEmpty_DoesNotThrow()
    {
        var existing = CreateParsedFile("existing.json", []);
        var newItem = CreateParsedFile("new.json", [CreateConversation("conv1", 100)]);

        var exception = Record.Exception(() => existing.RemoveSuperseded(newItem));

        Assert.Null(exception);
        Assert.Single(newItem.Conversations!);
    }

    private static ParsedConversationFile CreateParsedFile(string name, Conversations? conversations)
    {
        var fs = new MockFileSystem();
        var exportPath = MockUnixSupport.Path(@"C:\export");
        var path = MockUnixSupport.Path($@"C:\export\{name}");
        fs.AddDirectory(exportPath);
        fs.AddFile(path, new MockFileData("[]"));

        return new ParsedConversationFile
        {
            File = fs.FileInfo.New(path),
            ParseStatus = ConversationParseStatus.Success,
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
