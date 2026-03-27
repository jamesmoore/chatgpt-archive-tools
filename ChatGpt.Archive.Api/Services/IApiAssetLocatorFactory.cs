using ChatGPTExport.Assets;

namespace ChatGpt.Archive.Api.Services
{
    public interface IApiAssetLocatorFactory
    {
        IFileSystemAssetLocator Create();
    }
}
