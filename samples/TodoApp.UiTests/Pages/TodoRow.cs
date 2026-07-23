using Microsoft.Playwright;
using PlaywrightLibrary.Components;
using PlaywrightLibrary.Extensions;

namespace TodoApp.UiTests.Pages;

/// <summary>
/// Component object for a single todo row. It wraps the row's root locator and
/// resolves the title and toggle <em>relative to that root</em>, so it never
/// collides with other rows or the footer filters. It is locator-backed and holds
/// no captured state, so it stays valid as the list re-renders after each change.
/// </summary>
public sealed class TodoRow(ILocator root) : IComponent
{
    public ILocator Locator => root;

    // Resolved within the row, so they can never point at another row's elements.
    private ILocator TitleLabel => root.GetByTestId("todo-title");

    /// <summary>
    /// The row's completion checkbox. Exposed so tests can make web-first assertions
    /// on it via its locator, e.g. <c>Expect(row.Toggle.Locator).ToBeCheckedAsync()</c>.
    /// </summary>
    public CheckboxInput Toggle => new(root.GetByRole(AriaRole.Checkbox));

    public async Task<string> GetTitleAsync() => (await TitleLabel.InnerTextAsync()).Trim();

    public Task<bool> IsCompletedAsync() => Toggle.IsCheckedAsync();

    public Task CompleteAsync() => Toggle.SetAsync(true);

    public Task ReopenAsync() => Toggle.SetAsync(false);

    /// <summary>
    /// True when the title is rendered with a line-through — the completed styling.
    /// Uses the library's <c>HasComputedStyleAsync</c>, which walks up the DOM
    /// because the decoration is painted by an ancestor, not the title element.
    /// </summary>
    public Task<bool> IsStruckThroughAsync() => TitleLabel.HasComputedStyleAsync("textDecorationLine", "line-through");
}
