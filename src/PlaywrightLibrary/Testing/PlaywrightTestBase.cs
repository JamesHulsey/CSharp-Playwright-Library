using System.Collections.Concurrent;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace PlaywrightLibrary.Testing;

/// <summary>
/// Base class for Playwright tests. Tests run in parallel with a fresh fixture
/// instance per case; each mints its own browser context + page via
/// <see cref="CreateSessionAsync"/>, torn down automatically. The browser itself is
/// shared across tests (see <see cref="SharedBrowser"/>) — launched once, reused —
/// while every session gets an isolated context.
/// </summary>
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
[Parallelizable(ParallelScope.All)]
public abstract class PlaywrightTestBase(string url, TestOptions options)
{
    private readonly TestMediaHelper media = new(options);
    private readonly ConcurrentBag<PlaywrightSession> sessions = [];

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
        var browser = await SharedBrowser.GetAsync(options);

        var authFile = authOptions is null
            ? null
            : await PlaywrightAuthHelper.EnsureAuthStateAsync(url, authOptions);

        var context = await browser.NewContextAsync(new()
        {
            StorageStatePath = authFile,
            RecordVideoDir = media.GetOutputMediaDirectory(),
            RecordVideoSize = options.Video?.Size,
        });

        if (options.Trace is not null)
        {
            await context.Tracing.StartAsync(new()
            {
                Screenshots = options.Trace.Screenshots,
                Snapshots = options.Trace.Snapshots,
                Sources = options.Trace.Sources
            });
        }

        var page = await context.NewPageAsync();
        await page.GotoAsync(url);

        var session = new PlaywrightSession(context, page);
        sessions.Add(session);
        return session;
    }

    [TearDown]
    public async Task TearDownAsync()
    {
        var isFailed = TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed;

        foreach (var session in sessions)
        {
            if (options.Trace is not null)
                await media.StopTraceAsync(session.Context, isFailed);

            if (isFailed)
                await media.TakeScreenshotAsync(session.Page);

            await session.DisposeAsync();

            if (!isFailed)
                await media.DeleteVideoRecordingsAsync(session.Page);
        }

        // The browser is shared across tests and disposed at process exit — only the
        // per-test contexts are torn down here.
    }
}
