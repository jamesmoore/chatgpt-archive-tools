using ChatGPTExport.Decoders;
using ChatGPTExport.Models;

namespace ChatGTPExportTests.Decoders;

public class ContentCodeDecoderTests
{
    private static ContentCodeDecoder CreateDecoder(bool showHidden = false) => new(showHidden);

    private static MessageContext CreateContext(string recipient = "all")
    {
        return new MessageContext(new Author { role = "assistant" }, null, null, new MessageMetadata(), recipient);
    }

    [Fact]
    public void HiddenCode_IsFilteredWhenRecipientIsNotAll()
    {
        var decoder = CreateDecoder(showHidden: false);
        var content = new ContentCode { language = "csharp", text = "Console.WriteLine();" };
        var context = CreateContext(recipient: "not-all");

        var result = decoder.DecodeTo(content, context);

        Assert.Empty(result.Lines);
    }

    [Fact]
    public void UnknownLanguageSearch_IsRenderedAsWebSearch()
    {
        var decoder = CreateDecoder(showHidden: true);
        var content = new ContentCode { language = "unknown", text = "search(\"kittens\")" };
        var context = CreateContext();

        var result = decoder.DecodeTo(content, context);

        var line = Assert.Single(result.Lines);
        Assert.Equal("> 🔍 **Web search:** kittens.", line);
    }

    [Fact]
    public void UnknownLanguageJson_IsRenderedAsJsonCodeBlock()
    {
        var decoder = CreateDecoder(showHidden: true);
        var content = new ContentCode { language = "unknown", text = "{\"a\":1}" };
        var context = CreateContext();

        var result = decoder.DecodeTo(content, context);

        var line = Assert.Single(result.Lines);
        var expected = $"```json{Environment.NewLine}{content.text}{Environment.NewLine}```";
        Assert.Equal(expected, line);
    }
}
