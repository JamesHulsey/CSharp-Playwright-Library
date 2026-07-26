using Microsoft.Playwright;
using NUnit.Framework;
using Toolshop.Tests.Infrastructure;
using Toolshop.Tests.Pages;

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
        await Header.GoToSignInAsync();
        await LoginPage.LoginAsync("customer@practicesoftwaretesting.com", "welcome01");
        await Assertions.Expect(Header.AccountMenu).ToContainTextAsync("Jane Doe");

        // Add a product to the cart.
        await Page.GotoAsync(TestConfig.BaseUrl);
        await Catalog.Card("Combination Pliers").OpenAsync();
        await ProductDetailPage.Create(Page).AddToCartAsync();

        // Walk the checkout stepper to a placed order.
        await Header.GoToCartAsync();
        await Checkout.ProceedToCheckoutAsync();
        await Checkout.ContinueAsSignedInAsync();
        await Checkout.FillAddressAsync("US", "12345", "123 Test St", "New York", "NY");
        await Checkout.ProceedToPaymentAsync();
        await Checkout.PayByBankTransferAsync("Test Bank", "Jane Doe", "123456789");
        await Checkout.ConfirmOrderAsync();

        await Assertions.Expect(Checkout.SuccessMessage).ToBeVisibleAsync();
    }
}
