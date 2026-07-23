using NUnit.Framework;
using PlaywrightLibrary.Testing;

namespace TodoApp.UiTests.Infrastructure;

/// <summary>
/// Central run configuration for the suite. Values are read from the
/// <c>&lt;TestRunParameters&gt;</c> in <c>TodoApp.UiTests.runsettings</c> (exposed by
/// NUnit as <see cref="TestContext.Parameters"/>). Each value falls back to a
/// sensible default, so the tests still run if the settings file is absent — but
/// editing that file is the intended way to change how the suite runs.
/// </summary>
public static class TestConfig
{
    private static TestParameters Parameters => TestContext.Parameters;

    public static string BaseUrl => Parameters.Get("BaseUrl", "https://demo.playwright.dev/todomvc");

    public static string Browser => Parameters.Get("Browser", "chromium");

    /// <summary>Headless unless the <c>Headless</c> parameter is set to <c>false</c>.</summary>
    public static bool Headless => !string.Equals(Parameters.Get("Headless", "true"), "false", StringComparison.OrdinalIgnoreCase);

    /// <summary>Capture a Playwright trace (retained on failure) unless the <c>Trace</c> parameter is <c>false</c>.</summary>
    public static bool Trace => !string.Equals(Parameters.Get("Trace", "true"), "false", StringComparison.OrdinalIgnoreCase);

    public static TestOptions Options => new()
    {
        Environment = Parameters.Get("Environment", "local"),
        Browser = Browser,
        Headless = Headless,
        SlowMo = ParseSlowMo(Parameters.Get("SlowMo", "0")),
        Video = TestVideoOptions.Default,
        Trace = Trace ? TestTraceOptions.Default : null
    };

    /// <summary>Positive milliseconds slow down each action; anything else means full speed.</summary>
    private static float? ParseSlowMo(string value) =>
        float.TryParse(value, out var ms) && ms > 0 ? ms : null;
}
