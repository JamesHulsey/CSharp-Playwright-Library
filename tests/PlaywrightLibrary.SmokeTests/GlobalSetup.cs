using NUnit.Framework;

namespace PlaywrightLibrary.SmokeTests;

/// <summary>
/// Runs once before any test in this assembly and makes sure the Playwright
/// browser binaries are present.
///
/// It calls the same install routine as the <c>playwright.ps1 install</c>
/// command, but in-process — so it needs no PowerShell and behaves identically
/// on Windows, macOS, and Linux. The install is idempotent: if the browser is
/// already downloaded it just verifies it and returns immediately, so this adds
/// negligible time to a normal run. The payoff is that a fresh clone can run
/// <c>dotnet test</c> with no manual browser-install step.
///
/// Note: on Linux this installs the browser binary but NOT the OS-level
/// libraries it needs to launch. CI installs those separately with the
/// <c>--with-deps</c> flag (see <c>.github/workflows/ci.yml</c>).
/// </summary>
[SetUpFixture]
public class PlaywrightBrowserSetup
{
    [OneTimeSetUp]
    public void InstallBrowsers()
    {
        // Install only Chromium — the browser these smoke tests launch (see
        // TestOptions.Browser in SmokeTest.cs). If you point a test at "firefox"
        // or "webkit", add that name here too.
        var exitCode = Microsoft.Playwright.Program.Main(new[] { "install", "chromium" });

        if (exitCode != 0)
        {
            throw new InvalidOperationException(
                $"Playwright browser installation failed with exit code {exitCode}.");
        }
    }
}
