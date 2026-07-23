using Microsoft.Playwright;

namespace PlaywrightLibrary.Components;

/// <summary>
/// Anything that wraps a locator. Locator is exposed as an escape hatch for
/// web-first assertions (Assertions.Expect) and advanced Playwright operations.
/// </summary>
public interface IComponent
{
    ILocator Locator { get; }
}

public interface IPageComponent : IComponent
{
    IPage Page { get; }
}

/// <summary>
/// Implemented by components that can construct themselves from a page.
/// </summary>
public interface IPageLevelComponent<TSelf>
{
    static abstract TSelf Create(IPage page);
}
