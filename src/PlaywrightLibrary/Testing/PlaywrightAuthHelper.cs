using System.Collections.Concurrent;
using Microsoft.Playwright;

namespace PlaywrightLibrary.Testing;

internal static class PlaywrightAuthHelper
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> locks = new();

    /// <summary>
    /// Returns the path to a valid storage-state file, minting a fresh one via a headed
    /// login if the cache is missing or stale. Locked per auth file path so multiple
    /// roles can be cached concurrently within one process.
    /// </summary>
    public static async Task<string> EnsureAuthStateAsync(string url, PlaywrightAuthOptions authOptions)
    {
        var sem = locks.GetOrAdd(authOptions.AuthFilePath, _ => new SemaphoreSlim(1, 1));
        await sem.WaitAsync();
        try
        {
            if (File.Exists(authOptions.AuthFilePath))
            {
                var fileAge = DateTime.Now - File.GetLastWriteTime(authOptions.AuthFilePath);

                if (fileAge < authOptions.CacheLifetime)
                    return authOptions.AuthFilePath;

                File.Delete(authOptions.AuthFilePath);
            }

            using var playwright = await Microsoft.Playwright.Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = false });
            var context = await browser.NewContextAsync(new() { ViewportSize = ViewportSize.NoViewport });
            var page = await context.NewPageAsync();

            await page.GotoAsync(url);

            if (!await authOptions.LoginAction(page))
                throw new TimeoutException("Authentication did not complete within the expected timeout.");

            await context.StorageStateAsync(new() { Path = authOptions.AuthFilePath });
            return authOptions.AuthFilePath;
        }
        finally
        {
            sem.Release();
        }
    }
}
