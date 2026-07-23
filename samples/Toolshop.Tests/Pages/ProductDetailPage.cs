using Microsoft.Playwright;
using PlaywrightLibrary.Components;

namespace Toolshop.Tests.Pages;

/// <summary>Page object for a Toolshop product detail page (<c>/product/{id}</c>).</summary>
public sealed class ProductDetailPage(IPage page) : IPageLevelComponent<ProductDetailPage>
{
    public static ProductDetailPage Create(IPage page) => new(page);

    /// <summary>Product name and unit price, exposed for web-first assertions.</summary>
    public ILocator Name => page.Locator("[data-test='product-name']");
    public ILocator UnitPrice => page.Locator("[data-test='unit-price']");

    public ButtonComponent AddToCartButton => new(page.Locator("[data-test='add-to-cart']"));

    public async Task<string> GetNameAsync() => (await Name.InnerTextAsync()).Trim();

    public Task AddToCartAsync() => AddToCartButton.ClickAsync();
}
