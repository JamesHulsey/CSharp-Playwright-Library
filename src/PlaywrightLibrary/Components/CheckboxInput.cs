using Microsoft.Playwright;

namespace PlaywrightLibrary.Components;

public sealed class CheckboxInput(ILocator locator) : IComponent
{
    public ILocator Locator => locator;

    public async Task<bool> IsCheckedAsync() => await locator.IsCheckedAsync();

    public async Task SetAsync(bool check)
    {
        if (check)
            await locator.CheckAsync();
        else
            await locator.UncheckAsync();
    }
}
