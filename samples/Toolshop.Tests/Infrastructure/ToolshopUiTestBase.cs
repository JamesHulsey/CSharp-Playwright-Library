using Microsoft.Playwright;
using NUnit.Framework;
using PlaywrightLibrary.Testing;
using Toolshop.Tests.Components;
using Toolshop.Tests.Pages;

namespace Toolshop.Tests.Infrastructure;

/// <summary>
/// Base for Toolshop UI tests. Opens a session on the landing page before each test —
/// so tests start there without repeating navigation — and exposes only what is always
/// available where you are: the page, the persistent <see cref="SiteHeader"/>, and the
/// landing <see cref="ProductCatalogPage"/>. Other pages are reached through navigation
/// methods that return them (e.g. <c>Header.GoToSignInAsync()</c>). API-only tests use
/// <see cref="ToolshopTestBase"/> instead and never launch a browser.
/// </summary>
public abstract class ToolshopUiTestBase : ToolshopTestBase
{
    private PlaywrightSession session = null!;

    protected IPage Page => session.Page;

    protected SiteHeader Header => new(Page);
    protected ProductCatalogPage Catalog => ProductCatalogPage.Create(Page);

    [SetUp]
    public async Task OpenLandingPageAsync()
    {
        session = await CreateSessionAsync();
    }
}
