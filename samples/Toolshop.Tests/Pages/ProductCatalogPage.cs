using Microsoft.Playwright;
using PlaywrightLibrary.Components;
using PlaywrightLibrary.Extensions;
using Toolshop.Tests.Components;

namespace Toolshop.Tests.Pages;

/// <summary>
/// Page object for the Toolshop product catalog (home page). It composes the
/// library's components — a <see cref="TextInput"/> search box, a
/// <see cref="ButtonComponent"/>, and <see cref="CheckboxInput"/> category filters —
/// against the app's <c>data-test</c> selectors. (Toolshop uses <c>data-test</c>
/// rather than Playwright's default <c>data-testid</c>, so locators are explicit.)
/// </summary>
public sealed class ProductCatalogPage(IPage page) : IPageLevelComponent<ProductCatalogPage>
{
    private readonly TextInput searchInput = new(page.Locator("[data-test='search-query']"));

    public static ProductCatalogPage Create(IPage page) => new(page);

    public ButtonComponent SearchButton => new(page.Locator("[data-test='search-submit']"));

    /// <summary>The catalog's sort dropdown (e.g. "Name (A - Z)", "Price (Low - High)").</summary>
    public SelectComponent SortDropdown => new(page.Locator("[data-test='sort']"));

    /// <summary>Every product card (an anchor linking to the product detail page).</summary>
    public ILocator ProductCards => page.Locator("a[data-test^='product-']");

    /// <summary>Every product's name, exposed for web-first assertions.</summary>
    public ILocator ProductNames => page.Locator("[data-test='product-name']");

    public async Task SearchAsync(string query)
    {
        await searchInput.FillAsync(query);
        await SearchButton.ClickAsync();
    }

    /// <summary>Ticks a category filter by its visible name — independent of its id.</summary>
    public Task FilterByCategoryAsync(string categoryName) =>
        new CheckboxInput(page.GetByRole(AriaRole.Checkbox, new() { Name = categoryName })).SetAsync(true);

    /// <summary>Sorts the catalog by a dropdown option label (e.g. "Name (A - Z)").</summary>
    public Task SortByAsync(string optionLabel) => SortDropdown.SelectByLabelAsync(optionLabel);

    /// <summary>
    /// Waits until the product grid has reloaded to a non-empty result set matching a
    /// predicate. Searching and filtering are debounced XHR reloads, so this polls the
    /// live DOM rather than trusting a network heuristic.
    /// </summary>
    public Task WaitForResultsAsync(Func<IReadOnlyList<string>, bool> predicate) =>
        ProductNames.WaitForAsync(async names =>
        {
            if (await names.CountAsync() == 0)
                return false;
            return predicate(await names.AllInnerTextsAsync());
        });

    /// <summary>The card for a given product, matched by name within the grid.</summary>
    public ProductCard Card(string productName) =>
        new(ProductCards.Filter(new() { HasTextString = productName }));

    /// <summary>Every product card, fanned out from the grid via <c>EnumerateAsync</c>.</summary>
    public async Task<IReadOnlyList<ProductCard>> GetCardsAsync() =>
        (await ProductCards.EnumerateAsync()).Select(locator => new ProductCard(locator)).ToList();

    /// <summary>The visible product names, in order (via the library's EnumerateAsync).</summary>
    public async Task<IReadOnlyList<string>> GetProductNamesAsync()
    {
        var names = await ProductNames.EnumerateAsync();
        var results = new List<string>();
        foreach (var name in names)
            results.Add((await name.InnerTextAsync()).Trim());
        return results;
    }
}
