using ChatGPTExport.Decoders;
using ChatGPTExport.Models;

namespace ChatGTPExportTests.Decoders;

public class ContentComputerOutputDecoderTests
{
    [Fact]
    public void ComputerOutput_IsAlwaysEmpty()
    {
        var decoder = new ContentComputerOutputDecoder();
        var content = new ContentComputerOutput();
        var context = new MessageContext(new Author { role = "assistant" }, null, null, new MessageMetadata(), "all");

        var result = decoder.DecodeTo(content, context);

        Assert.Empty(result.Lines);
    }
}
