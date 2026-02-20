using ChatGPTExport.Decoders;
using ChatGPTExport.Models;

namespace ChatGTPExportTests.Decoders;

public class ContentExecutionOutputDecoderTests
{
    private static ContentExecutionOutputDecoder CreateDecoder(bool showHidden = false) => new(showHidden);

    private static MessageContext CreateContext()
    {
        return new MessageContext(new Author { role = "assistant" }, null, null, new MessageMetadata(), "all");
    }

    [Fact]
    public void HiddenExecutionOutput_IsFilteredWhenShowHiddenIsFalse()
    {
        var decoder = CreateDecoder(showHidden: false);
        var content = new ContentExecutionOutput { text = "output" };

        var result = decoder.DecodeToMarkdown(content, CreateContext());

        Assert.Empty(result.Lines);
    }

    [Fact]
    public void ExecutionOutput_IsRenderedAsCodeBlockWhenShowHiddenIsTrue()
    {
        var decoder = CreateDecoder(showHidden: true);
        var content = new ContentExecutionOutput { text = "output" };

        var result = decoder.DecodeToMarkdown(content, CreateContext());

        var line = Assert.Single(result.Lines);
        var expected = $"```{Environment.NewLine}{content.text}{Environment.NewLine}```";
        Assert.Equal(expected, line);
    }
}
