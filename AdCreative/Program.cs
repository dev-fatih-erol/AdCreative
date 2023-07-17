using AdCreative;
using AdCreative.Services;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

class Program
{
    private static readonly string folder = "outputs";
    public delegate void ProgressDelegate(int current, int total);
    public static event ProgressDelegate Progress;

    static async Task Main()
    {
        var serviceProvider = new ServiceCollection()
            .AddSingleton<IFileService, FileService>()
            .BuildServiceProvider();

        var _fileService = serviceProvider.GetService<IFileService>()
                           ?? throw new ArgumentNullException(nameof(IFileService));

        string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Input.json");
        string jsonData = File.ReadAllText(jsonPath);
        var input = JsonConvert.DeserializeObject<Input>(jsonData);

        var totalImages = input.Count;
        var parallelism = input.Parallelism;
        var downloadedImages = 0;

        Console.WriteLine("*****Results*****");

        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            DeleteFolder();
            Console.WriteLine(); // To move to a new line before image is remove
            Console.WriteLine("--Images removed");
            Console.WriteLine("--Download cancelled. Exiting...");
            Environment.Exit(0);

        };

        Progress += (current, total) =>
        {
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write($"Progress: {current}/{total}.");
        };

        Console.WriteLine($"Downloading {totalImages} images ({parallelism} parallel downloads at most)");

        var semaphoreSlim = new SemaphoreSlim(parallelism);
        for (int i = 1; i <= totalImages; i++)
        {
            await semaphoreSlim.WaitAsync();
            CreateFolder();
            var imageUrl = GetRandomImageUrl();
            var imagePath = Path.Combine($"{AppDomain.CurrentDomain.BaseDirectory}", input.SavePath, i.ToString());
            await _fileService.DownloadAsync(imageUrl, imagePath);
            downloadedImages++;
            Progress.Invoke(downloadedImages, totalImages);
            semaphoreSlim.Release();
        }

        Console.WriteLine(); // To move to a new line after progress is complete
        Console.WriteLine("--File download complete. Press Enter to exit.");
        Console.ReadLine();
    }

    /// <summary>
    /// Creates the output folder if it doesn't exist.
    /// </summary>
    private static void CreateFolder()
    {
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);
    }

    /// <summary>
    /// Deletes the output folder if it exists.
    /// </summary>
    private static void DeleteFolder()
    {
        if (Directory.Exists(folder))
            Directory.Delete(folder, true);
    }

    /// <summary>
    /// Get a URL for a random image from Lorem Picsum service.
    /// The image has a width of 200 pixels and a height of 300 pixels.
    /// </summary>
    /// <returns>A string representing the URL of the random image.</returns>
    private static string GetRandomImageUrl() => $"https://picsum.photos/200/300";
}