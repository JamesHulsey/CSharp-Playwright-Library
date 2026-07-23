using PlaywrightLibrary.Testing;
using TodoApp.UiTests.Pages;

namespace TodoApp.UiTests.Infrastructure;

/// <summary>
/// Base class for every TodoApp UI test. It wires the library's
/// <see cref="PlaywrightTestBase"/> to this project's <see cref="TestConfig"/> so
/// individual test classes stay focused on behavior, and provides a single entry
/// point for opening the app.
/// </summary>
public abstract class TodoAppTestBase() : PlaywrightTestBase(TestConfig.BaseUrl, TestConfig.Options)
{
    /// <summary>
    /// Opens a fresh, isolated session on the app and returns its page object.
    /// Call it more than once for multi-user scenarios — each call is its own
    /// browser context, disposed automatically in teardown.
    /// </summary>
    protected async Task<TodoPage> OpenTodoAppAsync()
    {
        var session = await CreateSessionAsync();
        return TodoPage.Create(session.Page);
    }
}
