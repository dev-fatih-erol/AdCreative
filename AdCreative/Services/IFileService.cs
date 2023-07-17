namespace AdCreative.Services
{
    public interface IFileService
	{
        Task DownloadAsync(string url, string filePath);
    }
}