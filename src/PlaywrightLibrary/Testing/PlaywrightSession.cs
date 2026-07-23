using Microsoft.Playwright;

namespace PlaywrightLibrary.Testing;

/// <summary>
/// A single browser context + page pair. One session per logical user in a test.
/// </summary>
public sealed class PlaywrightSession : IAsyncDisposable
{
    private readonly IBrowserContext context;
    private bool disposed;

    public IBrowserContext Context => context;
    public IPage Page { get; }

    internal PlaywrightSession(IBrowserContext context, IPage page)
    {
        this.context = context;
        Page = page;
    }

    public async ValueTask DisposeAsync()
    {
        if (disposed)
            return;
        disposed = true;

        await context.CloseAsync();
        await context.DisposeAsync();
    }
}
