using NUnit.Framework;
using Toolshop.Tests.Infrastructure;
using Toolshop.Tests.Model;

namespace Toolshop.Tests.ApiTests;

/// <summary>
/// API-only tests against the Toolshop REST API. These launch no browser — they use
/// the library's API request context through the typed <c>ToolshopApiClient</c>.
/// </summary>
[TestFixture]
public class ProductApiTests : ToolshopTestBase
{
    [Test]
    public async Task GetProducts_ReturnsAPaginatedCatalog()
    {
        var api = await CreateApiClientAsync();

        var products = await api.GetProductsAsync();

        Assert.That(products.Total, Is.GreaterThan(0));
        Assert.That(products.Data, Is.Not.Empty);
        Assert.That(products.Data, Has.All.Matches<Product>(
            p => !string.IsNullOrWhiteSpace(p.Name) && p.Price > 0));
    }

    [Test]
    public async Task GetProduct_ById_ReturnsThatProduct()
    {
        var api = await CreateApiClientAsync();
        var expected = (await api.GetProductsAsync()).Data[0];

        var fetched = await api.GetProductAsync(expected.Id);

        Assert.That(fetched.Id, Is.EqualTo(expected.Id));
        Assert.That(fetched.Name, Is.EqualTo(expected.Name));
    }

    [Test]
    public async Task SearchProducts_ReturnsOnlyMatchingProducts()
    {
        var api = await CreateApiClientAsync();

        var results = await api.SearchProductsAsync("pliers");

        Assert.That(results.Data, Is.Not.Empty);
        Assert.That(results.Data, Has.All.Matches<Product>(
            p => p.Name.Contains("pliers", StringComparison.OrdinalIgnoreCase)));
    }

    [Test]
    public async Task GetCategories_IncludesExpectedTopLevelCategory()
    {
        var api = await CreateApiClientAsync();

        var categories = await api.GetCategoriesAsync();

        Assert.That(categories.Select(c => c.Name), Does.Contain("Power Tools"));
    }
}
