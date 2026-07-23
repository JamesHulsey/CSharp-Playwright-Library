using Microsoft.Playwright;
using NUnit.Framework;
using TodoApp.UiTests.Infrastructure;
using TodoApp.UiTests.Model;

namespace TodoApp.UiTests.Tests;

/// <summary>
/// Demonstrates that sessions are isolated browser contexts — the foundation for
/// multi-user tests, where each logical user gets independent state.
/// </summary>
[TestFixture]
public class SessionIsolationTests : TodoAppTestBase
{
    [Test]
    public async Task SeparateSessions_DoNotShareState()
    {
        var aliceTodo = new TodoItem("Alice's todo");

        var aliceApp = await OpenTodoAppAsync();
        await aliceApp.AddTodoAsync(aliceTodo);
        await Assertions.Expect(aliceApp.Titles).ToHaveTextAsync([aliceTodo.Title]);

        // A second session is a fresh context, so it sees none of Alice's todos.
        var bobApp = await OpenTodoAppAsync();
        await Assertions.Expect(bobApp.Items).ToHaveCountAsync(0);
    }
}
