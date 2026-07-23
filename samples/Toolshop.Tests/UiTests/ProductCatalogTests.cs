using Microsoft.Playwright;
using NUnit.Framework;
using Toolshop.Tests.Infrastructure;

namespace Toolshop.Tests.UiTests;

/// <summary>
/// UI tests for the Toolshop product catalog, driven through the <c>Catalog</c> page
/// object. The landing page is opened by <see cref="ToolshopUiTestBase"/> before each
/// test. The app is an Angular SPA, so results are awaited with condition-based polls.
/// </summary>
/// <remarks>
/// Categorized <c>ExternalUi</c> and excluded from CI: practicesoftwaretesting.com
/// sits behind Cloudflare bot-protection, which blocks headless browsers from CI
/// data-center IPs, so the app never renders there. These tests are stable locally.
/// </remarks>
[Category("ExternalUi")]
[TestFixture]
public class ProductCatalogTests : ToolshopUiTestBase
{
    [Test]
    public async Task Catalog_LoadsProducts()
    {
        await Assertions.Expect(Catalog.ProductNames.First).ToBeVisibleAsync();
        Assert.That((await Catalog.GetProductNamesAsync()).Count, Is.GreaterThan(1));
    }

    [Test]
    public async Task Search_ShowsOnlyMatchingProducts()
    {
        await Catalog.SearchAsync("pliers");

        await Catalog.WaitForResultsAsync(
            names => names.All(n => n.Contains("pliers", StringComparison.OrdinalIgnoreCase)));

        var results = await Catalog.GetProductNamesAsync();
        Assert.That(results, Is.Not.Empty);
        Assert.That(results, Has.All.Matches<string>(
            n => n.Contains("pliers", StringComparison.OrdinalIgnoreCase)));
    }

    [Test]
    public async Task FilterByCategory_NarrowsTheCatalog()
    {
        await Assertions.Expect(Catalog.ProductNames.First).ToBeVisibleAsync();
        var unfilteredCount = await Catalog.ProductNames.CountAsync();

        await Catalog.FilterByCategoryAsync("Pliers");

        await Catalog.WaitForResultsAsync(names => names.Count < unfilteredCount);

        var filtered = await Catalog.GetProductNamesAsync();
        Assert.That(filtered, Is.Not.Empty);
        Assert.That(filtered.Count, Is.LessThan(unfilteredCount));
    }

    [Test]
    public async Task SortByName_OrdersProductsAlphabetically()
    {
        await Assertions.Expect(Catalog.ProductNames.First).ToBeVisibleAsync();

        await Catalog.SortByAsync("Name (A - Z)");

        await Catalog.WaitForResultsAsync(
            names => names.SequenceEqual(names.OrderBy(n => n, StringComparer.Ordinal)));

        var names = await Catalog.GetProductNamesAsync();
        Assert.That(names, Is.Ordered.Using<string>(StringComparer.Ordinal));
    }
}
