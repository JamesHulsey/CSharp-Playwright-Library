namespace PlaywrightLibrary.Testing;

/// <summary>
/// Enables Playwright tracing for a test's sessions. A trace is recorded for every
/// context and, on failure, written to disk and attached to the test result so it
/// can be opened in the Playwright trace viewer; on success it is discarded. This
/// mirrors Playwright's "retain-on-failure" trace mode — the richest debugging
/// artifact Playwright offers (DOM snapshots, network, console, and sources).
/// </summary>
public record TestTraceOptions
{
    public static TestTraceOptions Default => new();

    /// <summary>Root directory for trace output; shares the media directory layout.</summary>
    public string Directory { get; init; } = "test-failures";

    public bool Screenshots { get; init; } = true;
    public bool Snapshots { get; init; } = true;
    public bool Sources { get; init; } = true;
}
