using ChatGPTExport.Assets;

namespace ChatGPTExport.Decoders.AssetRenderer
{
    public interface IMarkdownAssetRenderer
    {
        string RenderAsset(FileSystemAsset? asset, string asset_pointer);
    }
}