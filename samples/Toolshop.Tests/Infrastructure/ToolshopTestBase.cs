using PlaywrightLibrary.Testing;
using Toolshop.Tests.Api;

namespace Toolshop.Tests.Infrastructure;

/// <summary>
/// Base class for Toolshop tests. Wires the library's <see cref="PlaywrightTestBase"/>
/// to this project's <see cref="TestConfig"/>. API-only tests extend this directly and
/// use <see cref="CreateApiClientAsync"/> — no browser is launched. UI tests extend
/// <see cref="ToolshopUiTestBase"/>, which opens a page before each test.
/// </summary>
public abstract class ToolshopTestBase() : PlaywrightTestBase(TestConfig.BaseUrl, TestConfig.Options)
{
    /// <summary>
    /// A typed client bound to a fresh API request context (disposed automatically in
    /// teardown). No browser is involved.
    /// </summary>
    protected async Task<ToolshopApiClient> CreateApiClientAsync() =>
        new(await CreateApiContextAsync(TestConfig.ApiBaseUrl));
}
