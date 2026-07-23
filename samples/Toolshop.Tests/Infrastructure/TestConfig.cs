using NUnit.Framework;
using PlaywrightLibrary.Testing;

namespace Toolshop.Tests.Infrastructure;

/// <summary>
/// Central run configuration, read from the <c>&lt;TestRunParameters&gt;</c> in
/// <c>Toolshop.Tests.runsettings</c> with sensible fallbacks. Note the two URLs:
/// the UI app and the REST API live on different hosts.
/// </summary>
public static class TestConfig
{
    private static TestParameters Parameters => TestContext.Parameters;

    public static string BaseUrl => Parameters.Get("BaseUrl", "https://practicesoftwaretesting.com");

    public static string ApiBaseUrl => Parameters.Get("ApiBaseUrl", "https://api.practicesoftwaretesting.com");

    public static string Browser => Parameters.Get("Browser", "chromium");

    public static bool Headless => !string.Equals(Parameters.Get("Headless", "true"), "false", StringComparison.OrdinalIgnoreCase);

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

    private static float? ParseSlowMo(string value) => float.TryParse(value, out var ms) && ms > 0 ? ms : null;
}
