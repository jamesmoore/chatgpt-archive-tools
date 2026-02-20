using ChatGPTExport.Decoders;
using ChatGPTExport.Models;

namespace ChatGTPExportTests.Decoders;

public class ContentMultimodalTextDecoderTests
{
    private sealed class TestAssetRenderer : IMarkdownAssetRenderer
    {
        public IEnumerable<string> RenderAsset(MessageContext context, string asset_pointer)
        {
            return [$"![asset]({asset_pointer})"];
        }
    }

    private static MessageContext CreateContext()
    {
        return new MessageContext(
            new Author { role = "assistant" },
            null,
            null,
            new MessageMetadata { image_gen_title = "Generated Title" },
            "all");
    }

    [Fact]
    public void ImageParts_AreRenderedWithMetadataAndHasImage()
    {
        var decoder = new ContentMultimodalTextDecoder(new TestAssetRenderer());
        var content = new ContentMultimodalText
        {
            parts =
            [
                new ContentMultimodalText.ContentMultimodalTextPartsContainer { StringValue = "Intro" },
                new ContentMultimodalText.ContentMultimodalTextPartsContainer
                {
                    ObjectValue = new ContentMultimodalText.ContentMultimodalTextParts
                    {
                        content_type = "image_asset_pointer",
                        asset_pointer = "asset://image",
                        size_bytes = 123,
                        width = 640,
                        height = 480
                    }
                }
            ]
        };

        var result = decoder.DecodeToMarkdown(content, CreateContext());

        var output = result.Lines.ToArray();
        Assert.Contains("Intro", output);
        Assert.Contains("![asset](asset://image)", output);
        Assert.Contains("*Generated Title*  ", output);
        Assert.Contains("**Size:** 123 **Dims:** 640x480  ", output);
        Assert.True(result.HasImage);
    }
}
