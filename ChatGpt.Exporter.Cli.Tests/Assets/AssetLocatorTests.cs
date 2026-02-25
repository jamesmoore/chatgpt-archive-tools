using ChatGpt.Exporter.Cli.Assets;
using ChatGPTExport.Assets;
using System.IO.Abstractions.TestingHelpers;

namespace ChatGpt.Exporter.Cli.Tests.Assets
{
    public class AssetLocatorTests
    {
        [Fact]
        public void Malformed_role_does_not_escape_destination()
        {
            var fs = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { MockUnixSupport.Path(@"c:\source\conversations.json"), new MockFileData("{ somedata: somevalue }") },
                { MockUnixSupport.Path(@"c:\source\img.png"), new MockFileData("data") }
            });
            fs.AddDirectory(MockUnixSupport.Path(@"c:\dest"));

            var conversationsFile = fs.FileInfo.New(MockUnixSupport.Path(@"c:\source\conversations.json"));
            var locator = new AssetLocator(ConversationAssets.FromConversationsFile(conversationsFile));

            var request = new AssetRequest("img.png", "../evil", null, null);

            var result = locator.GetMarkdownMediaAsset(request);

            Assert.NotNull(result);
            var markdownLink = result.GetMarkdownLink();
            Assert.Equal("![img.png](./unknown-assets/img.png)  ", markdownLink);
        }
    }
}
