using ChatGPTExport.Assets;

namespace ChatGPTExport.Formatters.Markdown
{
    public class MarkdownAssetRenderer(IAssetLocator assetLocator) : IMarkdownAssetRenderer
    {
        public IEnumerable<string> RenderAsset(ContentVisitorContext context, string asset_pointer)
        {
            var searchPattern = GetSearchPattern(asset_pointer);
            var markdownAsset = GetMediaAsset(context, searchPattern);

            if (markdownAsset != null)
            {
                yield return markdownAsset?.GetMarkdownLink();
            }
            else
            {
                // TODO prevent this from being displayed in the FTS indexing.
                Console.Error.WriteLine("\tUnable to find asset " + asset_pointer);
                yield return $"> ⚠️ **Warning:** Could not find asset: {asset_pointer}.";
            }
        }

        private static string GetSearchPattern(string assetPointer)
        {
            return assetPointer.Replace("sediment://", string.Empty).Replace("file-service://", string.Empty);
        }

        private Asset? GetMediaAsset(ContentVisitorContext context, string searchPattern)
        {
            return assetLocator.GetMarkdownMediaAsset(new AssetRequest(
                searchPattern,
                context.Role,
                context.CreatedDate,
                context.UpdatedDate)
                );
        }
    }
}
