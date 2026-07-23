using Microsoft.Playwright;

namespace PlaywrightLibrary.Testing;

public record PlaywrightAuthOptions
{
    /// <summary>
    /// Drives the login flow on a headed page. Return true once authentication has completed.
    /// </summary>
    public required Func<IPage, Task<bool>> LoginAction { get; init; }

    /// <summary>
    /// Where the storage-state JSON is cached. Use a distinct path per role.
    /// </summary>
    public string AuthFilePath { get; init; } = "auth-state.json";

    public TimeSpan CacheLifetime { get; init; } = TimeSpan.FromHours(12);
}
