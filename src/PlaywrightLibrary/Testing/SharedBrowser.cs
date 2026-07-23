using Microsoft.Playwright;

using PlaywrightFactory = Microsoft.Playwright.Playwright;

namespace PlaywrightLibrary.Testing;

/// <summary>
/// Owns a process-wide Playwright instance and the browsers shared across all
/// tests, keyed by launch configuration. Launching a browser is expensive, so it
/// is done once and reused; each test still creates its own context + page (cheap
/// and fully isolated). This is the recommended Playwright pattern.
///
/// The browsers are disposed once, when the test process exits. NUnit gives a
/// library no assembly-level teardown hook inside the consumer, so a process-exit
/// handler is the self-contained way to clean up.
/// </summary>
internal static class SharedBrowser
{
    private static readonly SemaphoreSlim gate = new(1, 1);
    private static readonly Dictionary<string, IBrowser> browsers = [];
    private static IPlaywright? playwright;
    private static bool exitHookRegistered;

    /// <summary>
    /// Returns the shared browser for the given options, launching it on first use.
    /// Concurrent callers with the same configuration get the same instance.
    /// </summary>
    public static async Task<IBrowser> GetAsync(TestOptions options)
    {
        var key = $"{options.Browser.ToLowerInvariant()}|{options.Headless}|{options.SlowMo}";

        await gate.WaitAsync();
        try
        {
            playwright ??= await PlaywrightFactory.CreateAsync();
            RegisterExitHook();

            if (!browsers.TryGetValue(key, out var browser))
            {
                browser = await LaunchAsync(playwright, options);
                browsers[key] = browser;
            }

            return browser;
        }
        finally
        {
            gate.Release();
        }
    }

    private static Task<IBrowser> LaunchAsync(IPlaywright pw, TestOptions options)
    {
        var launchOptions = new BrowserTypeLaunchOptions
        {
            Headless = options.Headless,
            SlowMo = options.SlowMo
        };

        return options.Browser.ToLowerInvariant() switch
        {
            "chrome" or "chromium" => pw.Chromium.LaunchAsync(launchOptions),
            "webkit" => pw.Webkit.LaunchAsync(launchOptions),
            "firefox" => pw.Firefox.LaunchAsync(launchOptions),
            _ => throw new InvalidOperationException($"Unsupported browser type: {options.Browser}.")
        };
    }

    private static void RegisterExitHook()
    {
        if (exitHookRegistered)
            return;

        exitHookRegistered = true;
        AppDomain.CurrentDomain.ProcessExit += (_, _) => DisposeAll();
    }

    private static void DisposeAll()
    {
        foreach (var browser in browsers.Values)
            browser.DisposeAsync().AsTask().GetAwaiter().GetResult();

        browsers.Clear();
        playwright?.Dispose();
        playwright = null;
    }
}
