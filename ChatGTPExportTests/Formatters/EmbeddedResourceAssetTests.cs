using ChatGPTExport.Formatters;

namespace ChatGTPExportTests.Formatters;

public class EmbeddedResourceAssetTests
{
    private const string ResourceName = "ChatGPTExport.Formatters.Html.Templates.Styles.tailwindcompiled.css";

    [Fact]
    public void EmbeddedResourceAsset_Name_ContainsHash()
    {
        var asset = new EmbeddedResourceAsset("/styles/tailwindcompiled.css", ResourceName, "text/css");

        // Name should be something like /styles/tailwindcompiled.{8hexchars}.css
        Assert.Matches(@"^/styles/tailwindcompiled\.[0-9a-f]{8}\.css$", asset.Name);
    }

    [Fact]
    public void EmbeddedResourceAsset_Name_IsDeterministic()
    {
        var asset1 = new EmbeddedResourceAsset("/styles/tailwindcompiled.css", ResourceName, "text/css");
        var asset2 = new EmbeddedResourceAsset("/styles/tailwindcompiled.css", ResourceName, "text/css");

        Assert.Equal(asset1.Name, asset2.Name);
    }

    [Fact]
    public void EmbeddedResourceAsset_Name_DiffersFromOriginalName()
    {
        var asset = new EmbeddedResourceAsset("/styles/tailwindcompiled.css", ResourceName, "text/css");

        Assert.NotEqual("/styles/tailwindcompiled.css", asset.Name);
    }
}
