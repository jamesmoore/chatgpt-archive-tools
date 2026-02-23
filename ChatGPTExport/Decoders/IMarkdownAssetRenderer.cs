using ChatGPTExport.Assets;

namespace ChatGPTExport.Decoders
{
    public interface IMarkdownAssetRenderer
    {
        IEnumerable<string> RenderAsset(Asset? asset, string asset_pointer);
    }
}