using Microsoft.Playwright;
using NUnit.Framework;
using Toolshop.Tests.Infrastructure;

namespace Toolshop.Tests.UiTests;

/// <summary>
/// UI tests for the Toolshop product catalog, driven through the
/// <c>ProductCatalogPage</c>. The app is an Angular SPA, so assertions are web-first
/// (auto-retrying) to absorb async rendering.
/// </summary>
[TestFixture]
public class ProductCatalogTests : ToolshopTestBase
{
    [Test]
    public async Task Catalog_LoadsProducts()
    {
        var catalog = await OpenCatalogAsync();

        await Assertions.Expect(catalog.ProductNames.First).ToBeVisibleAsync();
        Assert.That((await catalog.GetProductNamesAsync()).Count, Is.GreaterThan(1));
    }

    [Test]
    public async Task Search_ShowsOnlyMatchingProducts()
    {
        var catalog = await OpenCatalogAsync();
        await Assertions.Expect(catalog.ProductNames.First).ToBeVisibleAsync();

        await catalog.SearchAsync("pliers");

        // Poll until the reloaded grid holds only matching products.
        await catalog.WaitForResultsAsync(
            names => names.All(n => n.Contains("pliers", StringComparison.OrdinalIgnoreCase)));

        var results = await catalog.GetProductNamesAsync();
        Assert.That(results, Is.Not.Empty);
        Assert.That(results, Has.All.Matches<string>(
            n => n.Contains("pliers", StringComparison.OrdinalIgnoreCase)));
    }

    [Test]
    public async Task FilterByCategory_NarrowsTheCatalog()
    {
        var catalog = await OpenCatalogAsync();
        await Assertions.Expect(catalog.ProductNames.First).ToBeVisibleAsync();
        var unfilteredCount = await catalog.ProductNames.CountAsync();

        await catalog.FilterByCategoryAsync("Pliers");

        // Poll until the grid reloads to a smaller (non-empty) result set.
        await catalog.WaitForResultsAsync(names => names.Count < unfilteredCount);

        var filtered = await catalog.GetProductNamesAsync();
        Assert.That(filtered, Is.Not.Empty);
        Assert.That(filtered.Count, Is.LessThan(unfilteredCount));
    }
}
