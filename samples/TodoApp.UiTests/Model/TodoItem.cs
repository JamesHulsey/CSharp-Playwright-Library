namespace TodoApp.UiTests.Model;

/// <summary>
/// Test data describing a todo. A record so tests can declare todos as first-class
/// values, pass them around instead of bare strings, and compare them by value.
/// Deliberately minimal today — extra fields (due date, priority) would slot in
/// here without churning the call sites that pass a <see cref="TodoItem"/> around.
/// </summary>
public sealed record TodoItem(string Title);
