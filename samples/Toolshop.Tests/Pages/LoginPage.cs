using Microsoft.Playwright;
using PlaywrightLibrary.Components;

namespace Toolshop.Tests.Pages;

/// <summary>Page object for the Toolshop login form.</summary>
public sealed class LoginPage(IPage page) : IPageLevelComponent<LoginPage>
{
    private readonly TextInput emailInput = new(page.Locator("[data-test='email']"));
    private readonly TextInput passwordInput = new(page.Locator("[data-test='password']"));

    public static LoginPage Create(IPage page) => new(page);

    public ButtonComponent SubmitButton => new(page.Locator("[data-test='login-submit']"));

    /// <summary>The validation error shown for a rejected login, exposed for assertions.</summary>
    public ILocator Error => page.Locator("[data-test='login-error']");

    public async Task LoginAsync(string email, string password)
    {
        await emailInput.FillAsync(email);
        await passwordInput.FillAsync(password);
        await SubmitButton.ClickAsync();
    }
}
