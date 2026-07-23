using System.Globalization;
using Microsoft.Playwright;
using NUnit.Framework;

namespace PlaywrightLibrary.Testing;

internal class TestMediaHelper(TestOptions options)
{
    private static readonly Lock cleanupLock = new();
    private static bool hasCleanedUpVideos;

    private readonly DateTime testStartedAt = DateTime.Now;

    // Video and trace share one per-test output folder; whichever is enabled sets the root.
    private string? RootDirectory => options.Video?.Directory ?? options.Trace?.Directory;

    internal string? GetOutputMediaDirectory()
    {
        if (RootDirectory is null)
            return null;

        return Path.Combine(
            RootDirectory,
            testStartedAt.ToString("yyyy-MM-dd"),
            options.Environment,
            TestContext.CurrentContext.Test.Name,
            testStartedAt.ToString("HH.mm.ss"));
    }

    /// <summary>
    /// Deletes the video recording for a passing test and removes any empty parent
    /// directories up to the root video directory.
    /// </summary>
    internal async Task DeleteVideoRecordingsAsync(IPage page)
    {
        if (page.Video is null)
            return;

        var videoPath = await page.Video.PathAsync();
        if (!File.Exists(videoPath))
            return;

        await DeleteVideosAsync(videoPath);

        var directory = Path.GetDirectoryName(videoPath);
        while (directory is not null
            && Directory.Exists(directory)
            && !Directory.EnumerateFileSystemEntries(directory).Any()
            && directory != options.Video?.Directory)
        {
            Directory.Delete(directory);
            directory = Path.GetDirectoryName(directory);
        }
    }

    private static async Task DeleteVideosAsync(string videoPath)
    {
        // retry logic for webkit, which holds the file handle briefly after close
        const int maxRetries = 5;
        for (var i = 0; i < maxRetries; i++)
        {
            try
            {
                File.Delete(videoPath);
                break;
            }
            catch (IOException) when (i < maxRetries - 1)
            {
                await Task.Delay(500);
            }
        }
    }

    internal static void CleanOldDirectories(string videoDirectory, int daysToKeep = 1)
    {
        lock (cleanupLock)
        {
            if (hasCleanedUpVideos)
                return;

            try
            {
                if (!Directory.Exists(videoDirectory))
                    return;

                var cutoffDate = DateTime.Now.AddDays(-daysToKeep).Date;

                foreach (var dateDir in Directory.GetDirectories(videoDirectory))
                {
                    var dirName = Path.GetFileName(dateDir);

                    if (DateTime.TryParseExact(dirName, "yyyy-MM-dd", null, DateTimeStyles.None, out var dirDate)
                        && dirDate < cutoffDate)
                    {
                        try
                        {
                            Directory.Delete(dateDir, recursive: true);
                        }
                        catch (Exception)
                        {
                            // best effort cleanup
                        }
                    }
                }
            }
            finally
            {
                hasCleanedUpVideos = true;
            }
        }
    }

    internal async Task TakeScreenshotAsync(IPage page)
    {
        var screenshotDir = GetOutputMediaDirectory();
        if (screenshotDir is null)
            return;

        Directory.CreateDirectory(screenshotDir);
        var screenshotPath = Path.Combine(screenshotDir, "screenshot.png");
        await page.ScreenshotAsync(new() { Path = screenshotPath, FullPage = true });
        TestContext.AddTestAttachment(screenshotPath, "Screenshot on failure");
    }

    /// <summary>
    /// Stops tracing on the context. On failure the trace zip is written alongside
    /// the other media and attached to the test result; on success it is discarded.
    /// Must be called before the context is closed.
    /// </summary>
    internal async Task StopTraceAsync(IBrowserContext context, bool testFailed)
    {
        var outputDir = testFailed ? GetOutputMediaDirectory() : null;
        if (outputDir is null)
        {
            await context.Tracing.StopAsync();
            return;
        }

        Directory.CreateDirectory(outputDir);
        var tracePath = Path.Combine(outputDir, "trace.zip");
        await context.Tracing.StopAsync(new() { Path = tracePath });
        TestContext.AddTestAttachment(tracePath, "Playwright trace (open with: playwright show-trace)");
    }
}
