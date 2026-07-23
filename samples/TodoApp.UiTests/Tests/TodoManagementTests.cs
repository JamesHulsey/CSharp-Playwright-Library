using Microsoft.Playwright;
using NUnit.Framework;
using TodoApp.UiTests.Infrastructure;
using TodoApp.UiTests.Model;

namespace TodoApp.UiTests.Tests;

/// <summary>
/// Core todo behaviors, driven through the <c>TodoPage</c> page object and
/// <c>TodoRow</c> component objects, with todos passed around as <c>TodoItem</c>s.
/// </summary>
[TestFixture]
public class TodoManagementTests : TodoAppTestBase
{
    private static readonly TodoItem BuyMilk = new("Buy milk");
    private static readonly TodoItem WalkTheDog = new("Walk the dog");
    private static readonly TodoItem WriteTests = new("Write tests");

    [Test]
    public async Task AddingTodos_ShowsThemInTheListInOrder()
    {
        var todos = await OpenTodoAppAsync();

        await todos.AddTodosAsync(BuyMilk, WalkTheDog, WriteTests);

        // Web-first assertion (auto-retries until the list settles)...
        await Assertions.Expect(todos.Titles).ToHaveTextAsync([BuyMilk.Title, WalkTheDog.Title, WriteTests.Title]);

        // ...then round-trip the rows back into TodoItems and compare by value —
        // the record gives us structural equality, so no string juggling.
        TodoItem[] expected = [BuyMilk, WalkTheDog, WriteTests];
        Assert.That(await todos.GetTodoItemsAsync(), Is.EqualTo(expected));
    }

    [Test]
    public async Task CompletingATodo_MarksItDoneAndStrikesItThrough()
    {
        var todo = new TodoItem("Complete me");

        var todos = await OpenTodoAppAsync();
        await todos.AddTodoAsync(todo);

        var row = todos.Row(todo);
        await row.CompleteAsync();

        // This test deliberately verifies the completed state with BOTH assertion
        // styles, to make the difference between them explicit.

        // Playwright web-first assertion — it does NOT check just once: it re-queries
        // the live DOM and auto-retries until the checkbox reports checked, or the
        // timeout (default 5s) elapses. That built-in polling absorbs any async delay
        // before the UI settles, so no manual wait is needed.
        await Assertions.Expect(row.Toggle.Locator).ToBeCheckedAsync();

        // NUnit assertion — `await` reads the DOM once, capturing a point-in-time
        // snapshot as a bool; Assert.That then checks that captured value with no
        // polling or retry. If it isn't true at that instant, the test fails
        // immediately. Best for already-resolved values and non-UI logic.
        Assert.That(await row.IsStruckThroughAsync(), Is.True);
    }

    [Test]
    public async Task ReopeningACompletedTodo_ClearsTheStrikeThrough()
    {
        var todo = new TodoItem("Finish then reopen");

        var todos = await OpenTodoAppAsync();
        await todos.AddTodoAsync(todo);

        var row = todos.Row(todo);
        await row.CompleteAsync();
        Assert.That(await row.IsCompletedAsync(), Is.True);

        await row.ReopenAsync();

        // Group the two checks so a failure reports both facets of "active again",
        // not just the first one to fail.
        var isCompleted = await row.IsCompletedAsync();
        var isStruckThrough = await row.IsStruckThroughAsync();
        Assert.Multiple(() =>
        {
            Assert.That(isCompleted, Is.False, "re-opened todo should not be completed");
            Assert.That(isStruckThrough, Is.False, "re-opened todo should not be struck through");
        });
    }

    [Test]
    public async Task ClearCompleted_RemovesOnlyCheckedTodos()
    {
        var keepTodo = new TodoItem("Keep me");
        var removeTodo = new TodoItem("Remove me");

        var todos = await OpenTodoAppAsync();
        await todos.AddTodosAsync(keepTodo, removeTodo);

        await todos.Row(removeTodo).CompleteAsync();
        await todos.ClearCompletedAsync();

        await Assertions.Expect(todos.Titles).ToHaveTextAsync([keepTodo.Title]);
    }
}
