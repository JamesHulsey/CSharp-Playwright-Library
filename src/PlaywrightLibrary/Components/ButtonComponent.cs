using Microsoft.Playwright;

namespace PlaywrightLibrary.Components;

// A button modeled as a component: click it and read its state. It has no knowledge of what clicking
// leads to - the caller owns the return type. Click relies on Playwright's built-in actionability
// (waits for visible + stable + enabled), so there is no synchronization logic here.
public sealed class ButtonComponent(ILocator locator) : IComponent
{
    public ILocator Locator => locator;

    public async Task ClickAsync() => await locator.ClickAsync();

    public async Task<bool> IsVisibleAsync() => await locator.IsVisibleAsync();

    public async Task<bool> IsEnabledAsync() => await locator.IsEnabledAsync();

    public async Task<string> GetTextAsync() => (await locator.InnerTextAsync()).Trim();
}
