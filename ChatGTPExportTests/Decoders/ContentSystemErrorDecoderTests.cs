using ChatGPTExport.Decoders;
using ChatGPTExport.Models;

namespace ChatGTPExportTests.Decoders;

public class ContentSystemErrorDecoderTests
{
    private static ContentSystemErrorDecoder CreateDecoder() => new();

    private static MessageContext CreateContext(bool showHidden)
    {
        return new MessageContext(new Author { role = "assistant" }, null, null, new MessageMetadata(), "all", new ConversationContext(), showHidden);
    }

    [Fact]
    public void SystemErrors_AreFilteredWhenShowHiddenIsFalse()
    {
        var decoder = CreateDecoder();
        var content = new ContentSystemError { name = "Error", text = "Details" };

        var result = decoder.Decode(content, CreateContext(showHidden: false));

        Assert.Empty(result.Lines);
    }

    [Fact]
    public void SystemErrors_AreRenderedWhenShowHiddenIsTrue()
    {
        var decoder = CreateDecoder();
        var content = new ContentSystemError { name = "Error", text = "Details" };

        var result = decoder.Decode(content, CreateContext(showHidden: true));

        var line = Assert.Single(result.Lines);
        Assert.Equal("🔴 Error: Details", line);
    }
}
