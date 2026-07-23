using Microsoft.Playwright;
using NUnit.Framework;
using Toolshop.Tests.Infrastructure;

namespace Toolshop.Tests.Hybrid;

/// <summary>
/// Hybrid API + UI test — the payoff of having both layers. It reads the source of
/// truth from the REST API, then verifies the UI reflects it: filtering the catalog
/// by a category in the browser should show exactly the products the API returns for
/// that category. Extends <see cref="ToolshopUiTestBase"/> (UI) and uses the inherited
/// API client, so it drives a page and an API context together.
/// </summary>
/// <remarks>Categorized <c>ExternalUi</c> and excluded from CI (Cloudflare — see catalog tests).</remarks>
[Category("ExternalUi")]
[TestFixture]
public class CategoryFilterHybridTests : ToolshopUiTestBase
{
    [Test]
    public async Task FilteringByCategory_ShowsExactlyTheApiProducts()
    {
        const string category = "Pliers";

        // Source of truth from the API. Category ids rotate on reseed, so resolve the
        // id by name rather than hardcoding it.
        var api = await CreateApiClientAsync();
        var id = (await api.GetCategoriesAsync()).First(c => c.Name == category).Id;
        var expected = (await api.GetProductsByCategoryAsync(id)).Data.Select(p => p.Name).ToList();

        // Apply the same filter through the UI and wait for the grid to settle to the
        // expected size (the default catalog shows a different count, so this can't
        // pass against the pre-filter view).
        await Catalog.FilterByCategoryAsync(category);
        await Catalog.WaitForResultsAsync(names => names.Count == expected.Count);

        var shown = await Catalog.GetProductNamesAsync();
        Assert.That(shown, Is.EquivalentTo(expected));
    }
}
