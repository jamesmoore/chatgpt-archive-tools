namespace ChatGPTExport.Assets
{
    public record FileSystemAssetRequest(
        string SearchPattern, 
        string Role, 
        DateTimeOffset? CreatedDate, 
        DateTimeOffset? UpdatedDate);
}
