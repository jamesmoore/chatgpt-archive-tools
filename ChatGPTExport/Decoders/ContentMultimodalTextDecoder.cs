using ChatGPTExport.Formatters;
using ChatGPTExport.Models;

namespace ChatGPTExport.Decoders
{
    public class ContentMultimodalTextDecoder(IMarkdownAssetRenderer markdownAssetRenderer)
    {
        private const string ImageAssetPointer = "image_asset_pointer";

        public MarkdownContentResult DecodeToMarkdown(ContentMultimodalText content, MessageContext context)
        {
            var markdownContent = new List<string>();
            bool hasImage = false;
            foreach (var part in content.parts ?? [])
            {
                if (part.IsObject && part.ObjectValue != null)
                {
                    var mediaAssets = GetMarkdownMediaAsset(context, part.ObjectValue);
                    markdownContent.AddRange(mediaAssets);
                    hasImage = hasImage || part.ObjectValue.content_type == ImageAssetPointer;
                }
                else if (part.IsString && part.StringValue != null)
                {
                    markdownContent.Add(part.StringValue);
                }
            }
            return new MarkdownContentResult(markdownContent, null, hasImage);

        }

        private IEnumerable<string> GetMarkdownMediaAsset(MessageContext context, ContentMultimodalText.ContentMultimodalTextParts obj)
        {
            switch (obj.content_type)
            {
                case ImageAssetPointer when string.IsNullOrWhiteSpace(obj.asset_pointer) == false:
                    {
                        var asset_pointer = obj.asset_pointer;
                        var strings = markdownAssetRenderer.RenderAsset(context, asset_pointer);

                        foreach (var str in strings)
                        {
                            yield return str;
                        }

                        if (context.MessageMetadata.image_gen_title != null)
                        {
                            yield return $"*{context.MessageMetadata.image_gen_title}*  ";
                        }
                        yield return $"**Size:** {obj.size_bytes} **Dims:** {obj.width}x{obj.height}  ";
                        break;
                    }

                case "real_time_user_audio_video_asset_pointer" when string.IsNullOrWhiteSpace(obj.audio_asset_pointer?.asset_pointer) == false:
                    {
                        var asset_pointer = obj.audio_asset_pointer.asset_pointer;
                        var strings = markdownAssetRenderer.RenderAsset(context, asset_pointer);

                        foreach (var str in strings)
                        {
                            yield return str;
                        }
                        break;
                    }

                case "audio_asset_pointer" when string.IsNullOrWhiteSpace(obj.asset_pointer) == false:
                    {
                        var asset_pointer = obj.asset_pointer;
                        var strings = markdownAssetRenderer.RenderAsset(context, asset_pointer);

                        foreach (var str in strings)
                        {
                            yield return str;
                        }
                        break;
                    }

                case "audio_transcription" when string.IsNullOrWhiteSpace(obj.text) == false:
                    yield return $"*{obj.text}*  ";
                    break;
            }
        }

    }
}
