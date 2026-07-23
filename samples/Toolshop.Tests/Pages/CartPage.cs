using Microsoft.Playwright;
using PlaywrightLibrary.Components;

namespace Toolshop.Tests.Pages;

/// <summary>
/// Page object for the cart (the first step of <c>/checkout</c>). Exposes the line
/// items and total for assertions.
/// </summary>
public sealed class CartPage(IPage page) : IPageLevelComponent<CartPage>
{
    public static CartPage Create(IPage page) => new(page);

    /// <summary>The title of each line item in the cart.</summary>
    public ILocator ItemTitles => page.Locator("[data-test='product-title']");

    /// <summary>The cart total.</summary>
    public ILocator Total => page.Locator("[data-test='cart-total']");
}
