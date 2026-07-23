using Microsoft.Playwright;
using PlaywrightLibrary.Components;
using PlaywrightLibrary.Extensions;
using TodoApp.UiTests.Model;

namespace TodoApp.UiTests.Pages;

/// <summary>
/// Page object for the TodoMVC application. It owns the page-level locators and
/// actions and hands out <see cref="TodoRow"/> component objects for per-row
/// interaction. Tests talk to this type and to rows rather than to raw locators,
/// so a markup change is absorbed in one place.
/// </summary>
public sealed class TodoPage(IPage page) : IPageLevelComponent<TodoPage>
{
    private readonly TextInput newTodoField = new(page.GetByPlaceholder("What needs to be done?"));

    public ILocator Titles => page.GetByTestId("todo-title");

    public ILocator Items => page.GetByTestId("todo-item");

    public ButtonComponent ClearCompletedButton =>
        new(page.GetByRole(AriaRole.Button, new() { Name = "Clear completed" }));

    public static TodoPage Create(IPage page) => new(page);

    public async Task AddTodoAsync(TodoItem todo)
    {
        await newTodoField.FillAsync(todo.Title);
        await newTodoField.Locator.PressAsync("Enter");
    }

    public async Task AddTodosAsync(params TodoItem[] todos)
    {
        foreach (var todo in todos)
            await AddTodoAsync(todo);
    }

    public Task ClearCompletedAsync() => ClearCompletedButton.ClickAsync();

    public TodoRow Row(TodoItem todo) => new(Items.Filter(new() { HasTextString = todo.Title }));

    private async Task<IReadOnlyList<TodoRow>> GetRowsAsync() =>
        (await Items.EnumerateAsync()).Select(locator => new TodoRow(locator)).ToList();

    public async Task<IReadOnlyList<TodoItem>> GetTodoItemsAsync()
    {
        var rows = await GetRowsAsync();
        var items = new List<TodoItem>();
        foreach (var row in rows)
            items.Add(new TodoItem(await row.GetTitleAsync()));
        return items;
    }
}
