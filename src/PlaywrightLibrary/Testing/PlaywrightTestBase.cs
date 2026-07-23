using System.Collections.Concurrent;
using Microsoft.Playwright;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace PlaywrightLibrary.Testing;

/// <summary>
/// Base class for Playwright tests. Tests run in parallel with a fresh fixture
/// instance per case; each mints its own browser context + page via
/// <see cref="CreateSessionAsync"/>, torn down automatically. The browser itself is
/// shared across tests (see <see cref="SharedPlaywright"/>) — launched once, reused —
/// while every session gets an isolated context. API request contexts
/// (<see cref="CreateApiContextAsync"/>) are created independently — no browser.
/// </summary>
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
[Parallelizable(ParallelScope.All)]
public abstract class PlaywrightTestBase(string url, TestOptions options)
{
    private readonly TestMediaHelper media = new(options);
    private readonly ConcurrentBag<PlaywrightSession> sessions = [];
    private readonly ConcurrentBag<IAPIRequestContext> apiContexts = [];

    [SetUp]
    public void SetUp()
    {
        var artifactsDirectory = options.Video?.Directory ?? options.Trace?.Directory;
        if (artifactsDirectory is not null)
            TestMediaHelper.CleanOldDirectories(artifactsDirectory);
    }

    /// <summary>
    /// Creates a new browser context + page on the shared browser, optionally
    /// authenticated. Pass null for an anonymous session.
    /// </summary>
    protected async Task<PlaywrightSession> CreateSessionAsync(PlaywrightAuthOptions? authOptions = null)
    {
        var browser = await SharedPlaywright.GetBrowserAsync(options);

        var authFile = authOptions is null
            ? null
            : await PlaywrightAuthHelper.EnsureAuthStateAsync(url, authOptions);

        var context = await CreateContextAsync(browser, authFile);
        await media.StartTraceAsync(context);

        var page = await context.NewPageAsync();
        await page.GotoAsync(url);

        var session = new PlaywrightSession(context, page);
        sessions.Add(session);
        return session;
    }

    /// <summary>
    /// Creates a standalone API request context for API-only tests or for seeding
    /// and reading data alongside a UI test. Pass the API base URL (which may differ
    /// from the app URL). Disposed automatically in teardown.
    /// </summary>
    protected async Task<IAPIRequestContext> CreateApiContextAsync(string? baseUrl = null)
    {
        var apiContext = await SharedPlaywright.CreateApiContextAsync(baseUrl);
        apiContexts.Add(apiContext);
        return apiContext;
    }

    private Task<IBrowserContext> CreateContextAsync(IBrowser browser, string? authFile) =>
        browser.NewContextAsync(new()
        {
            StorageStatePath = authFile,
            RecordVideoDir = media.GetOutputMediaDirectory(),
            RecordVideoSize = options.Video?.Size,
        });

    [TearDown]
    public async Task TearDownAsync()
    {
        var isFailed = TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed;

        foreach (var session in sessions)
        {
            await media.StopTraceAsync(session.Context, isFailed);

            if (isFailed)
                await media.TakeScreenshotAsync(session.Page);

            await session.DisposeAsync();

            if (!isFailed)
                await media.DeleteVideoRecordingsAsync(session.Page);
        }

        foreach (var apiContext in apiContexts)
            await apiContext.DisposeAsync();

        // The browser is shared across tests and disposed at process exit — only the
        // per-test contexts are torn down here.
    }
}
