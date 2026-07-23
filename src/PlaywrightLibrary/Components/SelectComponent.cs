using Microsoft.Playwright;

namespace PlaywrightLibrary.Components;

// A native <select> dropdown modeled as a component. Selection relies on Playwright's
// built-in actionability (waits for the element to be ready), so there is no
// synchronization logic here.
public sealed class SelectComponent(ILocator locator) : IComponent
{
    public ILocator Locator => locator;

    public async Task SelectByLabelAsync(string label) =>
        await locator.SelectOptionAsync(new SelectOptionValue { Label = label });

    public async Task SelectByValueAsync(string value) =>
        await locator.SelectOptionAsync(new SelectOptionValue { Value = value });

    public async Task<string> GetSelectedLabelAsync() =>
        (await locator.Locator("option:checked").InnerTextAsync()).Trim();
}
