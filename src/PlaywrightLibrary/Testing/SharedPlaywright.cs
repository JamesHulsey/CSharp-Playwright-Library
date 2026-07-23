using Microsoft.Playwright;

using PlaywrightFactory = Microsoft.Playwright.Playwright;

namespace PlaywrightLibrary.Testing;

/// <summary>
/// Owns the single, process-wide Playwright driver and the resources spun from it.
/// Mirroring Playwright's own shape, it exposes two independent factories:
/// <see cref="GetBrowserAsync"/> for browsers and <see cref="CreateApiContextAsync"/>
/// for API request contexts. Neither depends on the other — an API-only test never
/// launches a browser.
///
/// Browsers are shared (keyed by launch configuration) because launching one is
/// expensive; each test still gets its own isolated context. Everything is disposed
/// once when the test process exits, since NUnit gives a library no assembly-level
/// teardown hook inside the consumer.
/// </summary>
internal static class SharedPlaywright
{
    private static readonly SemaphoreSlim gate = new(1, 1);
    private static readonly Dictionary<string, IBrowser> browsers = [];
    private static IPlaywright? playwright;
    private static bool exitHookRegistered;

    /// <summary>
    /// Returns the shared browser for the given options, launching it on first use.
    /// Concurrent callers with the same configuration get the same instance.
    /// </summary>
    public static async Task<IBrowser> GetBrowserAsync(TestOptions options)
    {
        var key = $"{options.Browser.ToLowerInvariant()}|{options.Headless}|{options.SlowMo}";

        await gate.WaitAsync();
        try
        {
            var pw = await EnsurePlaywrightAsync();

            if (!browsers.TryGetValue(key, out var browser))
            {
                browser = await LaunchAsync(pw, options);
                browsers[key] = browser;
            }

            return browser;
        }
        finally
        {
            gate.Release();
        }
    }

    /// <summary>
    /// Creates a standalone API request context — no browser involved. Used for
    /// API-only tests and for seeding/reading data alongside a UI test. The caller
    /// owns disposal.
    /// </summary>
    public static async Task<IAPIRequestContext> CreateApiContextAsync(string? baseUrl)
    {
        await gate.WaitAsync();
        try
        {
            var pw = await EnsurePlaywrightAsync();
            return await pw.APIRequest.NewContextAsync(new() { BaseURL = baseUrl });
        }
        finally
        {
            gate.Release();
        }
    }

    // Lazily creates the shared driver and registers cleanup. Callers hold the gate.
    private static async Task<IPlaywright> EnsurePlaywrightAsync()
    {
        playwright ??= await PlaywrightFactory.CreateAsync();
        RegisterExitHook();
        return playwright;
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
