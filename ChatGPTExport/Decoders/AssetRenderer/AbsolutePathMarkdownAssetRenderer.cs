using ChatGPTExport.Assets;

namespace ChatGPTExport.Decoders.AssetRenderer
{
    public class AbsolutePathMarkdownAssetRenderer : IMarkdownAssetRenderer
    {
        public string RenderAsset(FileSystemAsset? asset, string asset_pointer)
        {
            if (asset != null)
            {
                var markdownPath = "/" + string.Join("/", asset.PathSegments.Select(Uri.EscapeDataString));
                return $"![{asset.PathSegments.Last()}]({markdownPath})  ";
            }
            else
            {
                Console.Error.WriteLine("\tUnable to find asset " + asset_pointer);
                return $"> ⚠️ **Warning:** Could not find asset: {asset_pointer}.";
            }
        }
    }
}
