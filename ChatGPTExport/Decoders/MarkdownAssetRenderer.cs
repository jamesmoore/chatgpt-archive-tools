using ChatGPTExport.Assets;

namespace ChatGPTExport.Decoders
{
    public class RelativePathMarkdownAssetRenderer : IMarkdownAssetRenderer
    {
        public string RenderAsset(Asset? markdownAsset, string asset_pointer)
        {
            if (markdownAsset != null)
            {
                var markdownPath = "./" + string.Join("/",markdownAsset.PathSegments.Select(Uri.EscapeDataString));
                return $"![{markdownAsset.Name}]({markdownPath})  ";
            }
            else
            {
                // TODO prevent this from being displayed in the FTS indexing.
                Console.Error.WriteLine("\tUnable to find asset " + asset_pointer);
                return $"> ⚠️ **Warning:** Could not find asset: {asset_pointer}.";
            }
        }
    }

    public class AbsolutePathMarkdownAssetRenderer : IMarkdownAssetRenderer
    {
        public string RenderAsset(Asset? markdownAsset, string asset_pointer)
        {
            if (markdownAsset != null)
            {
                var markdownPath = "/" + string.Join("/", markdownAsset.PathSegments.Select(Uri.EscapeDataString));
                return $"![{markdownAsset.Name}]({markdownPath})  ";
            }
            else
            {
                // TODO prevent this from being displayed in the FTS indexing.
                Console.Error.WriteLine("\tUnable to find asset " + asset_pointer);
                return $"> ⚠️ **Warning:** Could not find asset: {asset_pointer}.";
            }
        }
    }
}
