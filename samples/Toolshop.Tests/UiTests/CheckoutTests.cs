using Microsoft.Playwright;
using NUnit.Framework;
using Toolshop.Tests.Infrastructure;

namespace Toolshop.Tests.UiTests;

/// <summary>
/// End-to-end checkout: sign in, add a product, and walk the whole checkout stepper
/// (cart → address → payment) to a placed order. The capstone journey — it exercises
/// login, the cart, the checkout page objects, and the library's SelectComponent.
/// </summary>
/// <remarks>Categorized <c>ExternalUi</c> and excluded from CI (Cloudflare — see catalog tests).</remarks>
[Category("ExternalUi")]
[TestFixture]
public class CheckoutTests : ToolshopUiTestBase
{
    // Long multi-step journey against a live external site — retry to absorb the
    // occasional transient timeout on a stepper transition. Each retry re-runs from a
    // fresh session.
    [Test]
    [Retry(2)]
    public async Task CompletingCheckout_AsASignedInCustomer_PlacesTheOrder()
    {
        // Sign in.
        var login = await Header.GoToSignInAsync();
        await login.LoginAsync("customer@practicesoftwaretesting.com", "welcome01");
        await Assertions.Expect(Header.AccountMenu).ToContainTextAsync("Jane Doe");

        // Add a product to the cart.
        await Page.GotoAsync(TestConfig.BaseUrl);
        var detail = await Catalog.Card("Combination Pliers").OpenAsync();
        await detail.AddToCartAsync();

        // Walk the checkout stepper to a placed order.
        var cart = await Header.GoToCartAsync();
        var checkout = await cart.ProceedToCheckoutAsync();
        await checkout.ContinueAsSignedInAsync();
        await checkout.FillAddressAsync("US", "12345", "123 Test St", "New York", "NY");
        await checkout.ProceedToPaymentAsync();
        await checkout.PayByBankTransferAsync("Test Bank", "Jane Doe", "123456789");
        await checkout.ConfirmOrderAsync();

        await Assertions.Expect(checkout.SuccessMessage).ToBeVisibleAsync();
    }
}
