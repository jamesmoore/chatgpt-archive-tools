using System.Text.Json;
using ChatGPTExport.Decoders;
using ChatGPTExport.Models;

namespace ChatGTPExportTests.Decoders;

public class UnhandledContentDecoderTests
{
    [Fact]
    public void UnhandledContent_IncludesExtraDataTable()
    {
        using var doc = JsonDocument.Parse("\"value\"");
        var content = new ContentComputerOutput
        {
            ExtraData = new Dictionary<string, JsonElement>
            {
                ["example"] = doc.RootElement.Clone()
            }
        };
        var decoder = new UnhandledContentDecoder();
        var context = new MessageContext(new Author { role = "assistant" }, null, null, new MessageMetadata(), "all");

        var result = decoder.DecodeTo(content, context);

        var output = string.Join("\n", result.Lines);
        Assert.Contains("Unhandled content type:", output);
        Assert.Contains("|Name|Value|", output);
        Assert.Contains("|example|\"value\"|", output);
    }
}
