namespace AdCreative.Services
{
    public class FileService : IFileService
	{
        // Note: There is no alternative method available for item 2.
        public async Task DownloadAsync(string url, string filePath)
        {
            using var httpClient = new HttpClient();

            var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

            if (response.IsSuccessStatusCode)
            {
                var stream = await response.Content.ReadAsStreamAsync();

                using var fileStream = new FileStream($"{filePath}.{Path.GetExtension(response.RequestMessage.RequestUri.AbsolutePath)}", FileMode.Create);

                await stream.CopyToAsync(fileStream);

                return;
            }

            throw new HttpRequestException("Failed to download file");
        }
    }
}