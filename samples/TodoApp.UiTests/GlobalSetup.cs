using NUnit.Framework;
using TodoApp.UiTests.Infrastructure;

namespace TodoApp.UiTests;

/// <summary>
/// Runs once before any test in this assembly and makes sure the Playwright
/// browser binaries are present. A consuming project owns this step itself —
/// it's the same routine as the <c>playwright.ps1 install</c> command, run
/// in-process so it needs no PowerShell and behaves identically on Windows,
/// macOS, and Linux. The install is idempotent, so a fresh clone can run
/// <c>dotnet test</c> with no manual browser-install step.
///
/// Note: on Linux this installs the browser binary but NOT the OS-level
/// libraries it needs to launch. CI installs those with the <c>--with-deps</c>
/// flag (see <c>.github/workflows/ci.yml</c>).
/// </summary>
[SetUpFixture]
public class PlaywrightBrowserSetup
{
    [OneTimeSetUp]
    public void InstallBrowsers()
    {
        // Install whichever browser the suite is configured to launch (Chromium by
        // default; change the Browser parameter in TodoApp.UiTests.runsettings).
        var exitCode = Microsoft.Playwright.Program.Main(new[] { "install", TestConfig.Browser });

        if (exitCode != 0)
        {
            throw new InvalidOperationException(
                $"Playwright browser installation failed with exit code {exitCode}.");
        }
    }
}
