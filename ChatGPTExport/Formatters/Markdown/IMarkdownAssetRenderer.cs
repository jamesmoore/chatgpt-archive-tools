namespace ChatGPTExport.Formatters.Markdown
{
    public interface IMarkdownAssetRenderer
    {
        IEnumerable<string> RenderAsset(ContentVisitorContext context, string asset_pointer);
    }
}