using ChatGPTExport.Assets;

namespace ChatGPTExport.Decoders
{
    public interface IMarkdownAssetRenderer
    {
        string RenderAsset(Asset? asset, string asset_pointer);
    }
}