using Microsoft.Playwright;
using NUnit.Framework;
using Toolshop.Tests.Infrastructure;

namespace Toolshop.Tests.UiTests;

/// <summary>
/// Cart tests: add a product from its detail page and confirm it lands in the cart.
/// Each test runs in a fresh session, so the cart starts empty.
/// </summary>
/// <remarks>Categorized <c>ExternalUi</c> and excluded from CI (Cloudflare — see catalog tests).</remarks>
[Category("ExternalUi")]
[TestFixture]
public class CartTests : ToolshopUiTestBase
{
    [Test]
    public async Task AddingAProductToTheCart_ShowsItInTheCart()
    {
        const string product = "Combination Pliers";
        var detail = await Catalog.Card(product).OpenAsync();

        await detail.AddToCartAsync();

        // The nav badge reflects the added item...
        await Assertions.Expect(Header.CartQuantity).ToHaveTextAsync("1");

        // ...and the cart lists it with a total.
        var cart = await Header.GoToCartAsync();
        await Assertions.Expect(cart.ItemTitles).ToContainTextAsync(product);
        await Assertions.Expect(cart.Total).ToContainTextAsync("$");
    }
}
