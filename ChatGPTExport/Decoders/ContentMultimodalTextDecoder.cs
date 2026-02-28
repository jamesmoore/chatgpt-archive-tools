using ChatGPTExport.Assets;
using ChatGPTExport.Decoders.AssetRenderer;
using ChatGPTExport.Models;

namespace ChatGPTExport.Decoders
{
    public class ContentMultimodalTextDecoder(IFileSystemAssetLocator assetLocator, IMarkdownAssetRenderer markdownAssetRenderer) : IDecoder<ContentMultimodalText, MarkdownContentResult>
    {
        private const string ImageAssetPointer = "image_asset_pointer";

        public MarkdownContentResult Decode(ContentMultimodalText content, MessageContext context)
        {
            var markdownContent = new List<string>();
            var markdownAssets = new List<FileSystemAsset>();
            bool hasImage = false;
            foreach (var part in content.parts ?? [])
            {
                if (part.IsObject && part.ObjectValue != null)
                {
                    var mediaAssets = GetMarkdownMediaAsset(context, part.ObjectValue);
                    markdownContent.AddRange(mediaAssets.MarkdownLines);
                    if (mediaAssets.MarkdownAsset != null)
                    {
                        markdownAssets.Add(mediaAssets.MarkdownAsset);
                    }
                    hasImage = hasImage || part.ObjectValue.content_type == ImageAssetPointer;
                }
                else if (part.IsString && part.StringValue != null)
                {
                    markdownContent.Add(part.StringValue);
                }
            }
            return new MarkdownContentResult(markdownContent, markdownAssets, null, hasImage);

        }

        private (IEnumerable<string> MarkdownLines, FileSystemAsset? MarkdownAsset) GetMarkdownMediaAsset(MessageContext context, ContentMultimodalText.ContentMultimodalTextParts obj)
        {
            switch (obj.content_type)
            {
                case ImageAssetPointer when string.IsNullOrWhiteSpace(obj.asset_pointer) == false:
                    {
                        var asset_pointer = obj.asset_pointer;
                        var asset = GetAsset(context, asset_pointer);
                        var assetString = markdownAssetRenderer.RenderAsset(asset, asset_pointer);

                        var mediaAssets = new List<string>
                        {
                            assetString
                        };

                        if (context.MessageMetadata.image_gen_title != null)
                        {
                            mediaAssets.Add($"*{context.MessageMetadata.image_gen_title}*  ");
                        }
                        mediaAssets.Add($"**Size:** {obj.size_bytes} **Dims:** {obj.width}x{obj.height}  ");
                        return (mediaAssets, asset);
                    }
                case "real_time_user_audio_video_asset_pointer" when string.IsNullOrWhiteSpace(obj.audio_asset_pointer?.asset_pointer) == false:
                    {
                        var asset_pointer = obj.audio_asset_pointer.asset_pointer;
                        var asset = GetAsset(context, asset_pointer);
                        var assetString = markdownAssetRenderer.RenderAsset(asset, asset_pointer);

                        return ([assetString], asset);
                    }
                case "audio_asset_pointer" when string.IsNullOrWhiteSpace(obj.asset_pointer) == false:
                    {
                        var asset_pointer = obj.asset_pointer;
                        var asset = GetAsset(context, asset_pointer);
                        var assetString = markdownAssetRenderer.RenderAsset(asset, asset_pointer);

                        return ([assetString], asset);
                    }
                case "audio_transcription" when string.IsNullOrWhiteSpace(obj.text) == false:
                    return ([$"*{obj.text}*  "], null);
                default:
                    return (Enumerable.Empty<string>(), null);
            }
        }

        private FileSystemAsset? GetAsset(MessageContext context, string asset_pointer)
        {
            var searchPattern = asset_pointer
                .Replace("sediment://", string.Empty)
                .Replace("file-service://", string.Empty);
            FileSystemAssetRequest assetRequest = new(
                            searchPattern,
                            context.Role,
                            context.CreatedDate,
                            context.UpdatedDate);
            return assetLocator.GetMarkdownMediaAsset(assetRequest);
        }
    }
}
