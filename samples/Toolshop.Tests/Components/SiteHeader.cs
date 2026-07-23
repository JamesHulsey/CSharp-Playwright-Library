using Microsoft.Playwright;
using PlaywrightLibrary.Components;

namespace Toolshop.Tests.Components;

/// <summary>
/// The site header/nav, present on every page. Encapsulates the sign-in entry point
/// and the signed-in account menu, so tests can read auth state and navigate without
/// knowing the nav's markup. Composes the library's <see cref="ButtonComponent"/>.
/// </summary>
public sealed class SiteHeader(IPage page)
{
    public ButtonComponent SignInLink => new(page.Locator("[data-test='nav-sign-in']"));

    public ButtonComponent SignOutLink => new(page.Locator("[data-test='nav-sign-out']"));

    /// <summary>The account menu — shown only when signed in, labelled with the user's name.</summary>
    public ILocator AccountMenu => page.Locator("[data-test='nav-menu']");

    public Task GoToSignInAsync() => SignInLink.ClickAsync();

    public Task<bool> IsSignedInAsync() => AccountMenu.IsVisibleAsync();

    public async Task<string> SignedInUserAsync() => (await AccountMenu.InnerTextAsync()).Trim();
}
