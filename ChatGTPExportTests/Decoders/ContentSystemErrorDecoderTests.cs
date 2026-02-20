using ChatGPTExport.Decoders;
using ChatGPTExport.Models;

namespace ChatGTPExportTests.Decoders;

public class ContentSystemErrorDecoderTests
{
    private static ContentSystemErrorDecoder CreateDecoder(bool showHidden = false) => new(showHidden);

    private static MessageContext CreateContext()
    {
        return new MessageContext(new Author { role = "assistant" }, null, null, new MessageMetadata(), "all");
    }

    [Fact]
    public void SystemErrors_AreFilteredWhenShowHiddenIsFalse()
    {
        var decoder = CreateDecoder(showHidden: false);
        var content = new ContentSystemError { name = "Error", text = "Details" };

        var result = decoder.DecodeToMarkdown(content, CreateContext());

        Assert.Empty(result.Lines);
    }

    [Fact]
    public void SystemErrors_AreRenderedWhenShowHiddenIsTrue()
    {
        var decoder = CreateDecoder(showHidden: true);
        var content = new ContentSystemError { name = "Error", text = "Details" };

        var result = decoder.DecodeToMarkdown(content, CreateContext());

        var line = Assert.Single(result.Lines);
        Assert.Equal("🔴 Error: Details", line);
    }
}
