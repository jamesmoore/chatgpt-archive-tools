using ChatGPTExport.Assets;

namespace ChatGPTExport.Decoders.AssetRenderer
{
    public interface IMarkdownAssetRenderer
    {
        string RenderAsset(Asset? asset, string asset_pointer);
    }
}