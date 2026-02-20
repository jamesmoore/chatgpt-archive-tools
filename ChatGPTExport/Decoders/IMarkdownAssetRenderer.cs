namespace ChatGPTExport.Decoders
{
    public interface IMarkdownAssetRenderer
    {
        IEnumerable<string> RenderAsset(MessageContext context, string asset_pointer);
    }
}