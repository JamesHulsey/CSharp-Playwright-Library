using PlaywrightLibrary.Testing;
using Toolshop.Tests.Api;
using Toolshop.Tests.Pages;

namespace Toolshop.Tests.Infrastructure;

/// <summary>
/// Base class for every Toolshop test. Wires the library's
/// <see cref="PlaywrightTestBase"/> to this project's <see cref="TestConfig"/> and
/// exposes convenient entry points. API-only tests use <see cref="CreateApiClientAsync"/>
/// and never launch a browser; UI/hybrid tests additionally open pages.
/// </summary>
public abstract class ToolshopTestBase() : PlaywrightTestBase(TestConfig.BaseUrl, TestConfig.Options)
{
    /// <summary>
    /// A typed client bound to a fresh API request context (disposed automatically in
    /// teardown). No browser is involved.
    /// </summary>
    protected async Task<ToolshopApiClient> CreateApiClientAsync() =>
        new(await CreateApiContextAsync(TestConfig.ApiBaseUrl));

    /// <summary>Opens a UI session on the catalog (home) page and returns its page object.</summary>
    protected async Task<ProductCatalogPage> OpenCatalogAsync()
    {
        var session = await CreateSessionAsync();
        return ProductCatalogPage.Create(session.Page);
    }
}
