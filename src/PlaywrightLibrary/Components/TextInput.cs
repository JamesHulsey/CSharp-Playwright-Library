using Microsoft.Playwright;

namespace PlaywrightLibrary.Components;

public sealed class TextInput(ILocator locator) : IComponent
{
    public ILocator Locator => locator;

    public async Task ClearAsync() => await Locator.ClearAsync();

    public async Task FillAsync(string text) => await Locator.FillAsync(text);

    public async Task<string> GetValueAsync() => await Locator.InputValueAsync();
}
