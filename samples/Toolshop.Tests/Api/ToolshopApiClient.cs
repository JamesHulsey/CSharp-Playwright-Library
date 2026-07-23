using System.Text.Json;
using Microsoft.Playwright;
using Toolshop.Tests.Model;

namespace Toolshop.Tests.Api;

/// <summary>
/// A typed client over the Toolshop REST API — the API counterpart to a page object.
/// It wraps an <see cref="IAPIRequestContext"/> and returns strongly-typed models
/// rather than raw JSON, so tests read the domain, not HTTP plumbing.
/// </summary>
public sealed class ToolshopApiClient(IAPIRequestContext api)
{
    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true
    };

    public Task<ProductList> GetProductsAsync(int page = 1) =>
        GetAsync<ProductList>("/products", new() { ["page"] = page });

    public Task<Product> GetProductAsync(string id) =>
        GetAsync<Product>($"/products/{id}");

    public Task<ProductList> SearchProductsAsync(string query) =>
        GetAsync<ProductList>("/products/search", new() { ["q"] = query });

    public Task<ProductList> GetProductsByCategoryAsync(string categoryId) =>
        GetAsync<ProductList>("/products", new() { ["by_category"] = categoryId });

    public async Task<IReadOnlyList<Category>> GetCategoriesAsync()
    {
        var response = await api.GetAsync("/categories");
        EnsureOk(response, "/categories");
        return await DeserializeAsync<List<Category>>(response);
    }

    private async Task<T> GetAsync<T>(string path, Dictionary<string, object>? query = null)
    {
        var options = query is null ? null : new APIRequestContextOptions { Params = query };
        var response = await api.GetAsync(path, options);
        EnsureOk(response, path);
        return await DeserializeAsync<T>(response);
    }

    private static void EnsureOk(IAPIResponse response, string path)
    {
        if (!response.Ok)
            throw new InvalidOperationException($"GET {path} failed: HTTP {response.Status} {response.StatusText}.");
    }

    private static async Task<T> DeserializeAsync<T>(IAPIResponse response)
    {
        var body = await response.TextAsync();
        return JsonSerializer.Deserialize<T>(body, Json)
            ?? throw new InvalidOperationException($"Could not deserialize the response to {typeof(T).Name}.");
    }
}
