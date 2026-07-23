using Microsoft.Playwright;
using PlaywrightLibrary.Components;

namespace Toolshop.Tests.Components;

/// <summary>
/// Component object for a single product card in the catalog grid. Wraps the card's
/// root anchor and resolves its name and price relative to that root, and opens the
/// product's detail page. Locator-backed, so it stays valid as the grid re-renders.
/// </summary>
public sealed class ProductCard(ILocator root) : IComponent
{
    public ILocator Locator => root;

    private ILocator Name => root.Locator("[data-test='product-name']");
    private ILocator Price => root.Locator("[data-test='product-price']");

    public async Task<string> GetNameAsync() => (await Name.InnerTextAsync()).Trim();

    public async Task<string> GetPriceAsync() => (await Price.InnerTextAsync()).Trim();

    /// <summary>Opens the product's detail page (the card is a link).</summary>
    public Task OpenAsync() => root.ClickAsync();
}
