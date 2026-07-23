using Microsoft.Playwright;
using NUnit.Framework;
using PlaywrightLibrary.Testing;
using Toolshop.Tests.Components;
using Toolshop.Tests.Infrastructure;

namespace Toolshop.Tests.UiTests;

/// <summary>
/// Demonstrates the library's storage-state auth caching. The first run performs a
/// real login (in a headed browser) via <c>LoginAction</c> and caches the session to
/// disk; subsequent authenticated tests reuse that cache and start already signed in,
/// skipping the login UI entirely. This extends <see cref="ToolshopTestBase"/> (not the
/// UI base) because it creates its own authenticated session rather than an anonymous one.
/// </summary>
/// <remarks>Categorized <c>ExternalUi</c> and excluded from CI (Cloudflare — see catalog tests).</remarks>
[Category("ExternalUi")]
[TestFixture]
public class AuthStateTests : ToolshopTestBase
{
    // Reused across authenticated tests; the storage state is cached per AuthFilePath.
    private static PlaywrightAuthOptions CustomerAuth => new()
    {
        AuthFilePath = "auth-state.customer.json",
        LoginAction = async page =>
        {
            // The helper has already navigated to the landing page.
            await page.Locator("[data-test='nav-sign-in']").ClickAsync();
            await page.Locator("[data-test='email']").FillAsync("customer@practicesoftwaretesting.com");
            await page.Locator("[data-test='password']").FillAsync("welcome01");
            await page.Locator("[data-test='login-submit']").ClickAsync();

            var accountMenu = page.Locator("[data-test='nav-menu']");
            await accountMenu.WaitForAsync(new() { Timeout = 15_000 });
            return await accountMenu.IsVisibleAsync();
        }
    };

    [Test]
    public async Task AuthenticatedSession_StartsAlreadySignedIn()
    {
        // The session loads the cached storage state, so the landing page is already
        // authenticated — no login UI is driven here.
        var session = await CreateSessionAsync(CustomerAuth);

        var header = new SiteHeader(session.Page);
        await Assertions.Expect(header.AccountMenu).ToContainTextAsync("Jane Doe");
    }
}
