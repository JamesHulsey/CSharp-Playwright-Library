using NUnit.Framework;
using Toolshop.Tests.Infrastructure;

namespace Toolshop.Tests;

/// <summary>
/// Installs the Playwright browser once before any test in this assembly, the same
/// way as the sibling samples. API-only tests do not launch a browser, but this
/// suite also has UI and hybrid tests, so the browser is required.
/// </summary>
[SetUpFixture]
public class PlaywrightBrowserSetup
{
    [OneTimeSetUp]
    public void InstallBrowsers()
    {
        var exitCode = Microsoft.Playwright.Program.Main(new[] { "install", TestConfig.Browser });

        if (exitCode != 0)
        {
            throw new InvalidOperationException(
                $"Playwright browser installation failed with exit code {exitCode}.");
        }
    }
}
