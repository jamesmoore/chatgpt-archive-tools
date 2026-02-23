using ChatGPTExport.Decoders;
using ChatGPTExport.Models;

namespace ChatGTPExportTests.Decoders;

public class ContentExecutionOutputDecoderTests
{
    private static ContentExecutionOutputDecoder CreateDecoder() => new();

    private static MessageContext CreateContext(bool showHidden)
    {
        return new MessageContext(new Author { role = "assistant" }, null, null, new MessageMetadata(), "all", new ConversationContext(), showHidden);
    }

    [Fact]
    public void HiddenExecutionOutput_IsFilteredWhenShowHiddenIsFalse()
    {
        var decoder = CreateDecoder();
        var content = new ContentExecutionOutput { text = "output" };

        var result = decoder.Decode(content, CreateContext(showHidden: false));

        Assert.Empty(result.Lines);
    }

    [Fact]
    public void ExecutionOutput_IsRenderedAsCodeBlockWhenShowHiddenIsTrue()
    {
        var decoder = CreateDecoder();
        var content = new ContentExecutionOutput { text = "output" };

        var result = decoder.Decode(content, CreateContext(showHidden: true));

        var line = Assert.Single(result.Lines);
        var expected = $"```{Environment.NewLine}{content.text}{Environment.NewLine}```";
        Assert.Equal(expected, line);
    }
}
