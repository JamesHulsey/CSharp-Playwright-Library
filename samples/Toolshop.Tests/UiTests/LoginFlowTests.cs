using Microsoft.Playwright;
using NUnit.Framework;
using Toolshop.Tests.Infrastructure;

namespace Toolshop.Tests.UiTests;

/// <summary>
/// Exercises the login flow through the UI: from the landing page, sign in and confirm
/// the account menu appears. Uses the <see cref="Components.SiteHeader"/> for auth state
/// and the <c>LoginPage</c> for the form.
/// </summary>
/// <remarks>Categorized <c>ExternalUi</c> and excluded from CI (Cloudflare — see catalog tests).</remarks>
[Category("ExternalUi")]
[TestFixture]
public class LoginFlowTests : ToolshopUiTestBase
{
    private const string CustomerEmail = "customer@practicesoftwaretesting.com";
    private const string CustomerPassword = "welcome01";

    [Test]
    public async Task SigningIn_WithValidCredentials_ShowsTheAccountMenu()
    {
        await Header.GoToSignInAsync();

        await LoginPage.LoginAsync(CustomerEmail, CustomerPassword);

        await Assertions.Expect(Header.AccountMenu).ToContainTextAsync("Jane Doe");
    }
}
