using ChatGPTExport.Assets;

namespace ChatGPTExport.Decoders
{
    public class MarkdownAssetRenderer : IMarkdownAssetRenderer
    {
        public IEnumerable<string> RenderAsset(Asset? markdownAsset, string asset_pointer)
        {
            if (markdownAsset != null)
            {
                yield return markdownAsset.GetMarkdownLink();
            }
            else
            {
                // TODO prevent this from being displayed in the FTS indexing.
                Console.Error.WriteLine("\tUnable to find asset " + asset_pointer);
                yield return $"> ⚠️ **Warning:** Could not find asset: {asset_pointer}.";
            }
        }


    }
}
