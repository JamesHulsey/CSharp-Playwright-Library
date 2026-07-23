using System.Text.RegularExpressions;
using Microsoft.Playwright;
using NUnit.Framework;
using Toolshop.Tests.Infrastructure;
using Toolshop.Tests.Pages;

namespace Toolshop.Tests.UiTests;

/// <summary>
/// Tests that use <c>ProductCard</c> component objects and the <c>ProductDetailPage</c>.
/// </summary>
/// <remarks>Categorized <c>ExternalUi</c> and excluded from CI (Cloudflare — see catalog tests).</remarks>
[Category("ExternalUi")]
[TestFixture]
public class ProductDetailTests : ToolshopUiTestBase
{
    [Test]
    public async Task OpeningAProduct_ShowsItsDetailPage()
    {
        const string productName = "Combination Pliers";
        var card = Catalog.Card(productName);
        await Assertions.Expect(card.Locator).ToBeVisibleAsync();

        await card.OpenAsync();

        var detail = ProductDetailPage.Create(Page);
        await Assertions.Expect(detail.Name).ToHaveTextAsync(productName);
        await Assertions.Expect(detail.AddToCartButton.Locator).ToBeVisibleAsync();
    }

    [Test]
    public async Task EveryCard_ShowsANameAndPrice()
    {
        await Assertions.Expect(Catalog.ProductNames.First).ToBeVisibleAsync();

        var cards = await Catalog.GetCardsAsync();
        Assert.That(cards, Is.Not.Empty);

        foreach (var card in cards)
        {
            Assert.That(await card.GetNameAsync(), Is.Not.Empty);
            Assert.That(await card.GetPriceAsync(), Does.Match(new Regex(@"\d")));
        }
    }
}
