namespace Toolshop.Tests.Model;

// Typed models for the Toolshop API. Only the fields the tests use are declared;
// the JSON deserializer ignores the rest. Snake_case JSON keys (in_stock,
// current_page, ...) are mapped by the naming policy configured in ToolshopApiClient.

public sealed record Product(string Id, string Name, decimal Price, bool InStock, Category Category, Brand Brand);

public sealed record Category(string Id, string Name, string Slug);

public sealed record Brand(string Id, string Name);

/// <summary>One page of the paginated <c>/products</c> response.</summary>
public sealed record ProductList(IReadOnlyList<Product> Data, int Total, int CurrentPage, int LastPage, int PerPage);
