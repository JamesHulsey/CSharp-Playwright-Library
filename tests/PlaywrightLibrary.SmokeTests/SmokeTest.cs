using Microsoft.Playwright;
using NUnit.Framework;
using PlaywrightLibrary.Components;
using PlaywrightLibrary.Testing;

namespace PlaywrightLibrary.SmokeTests;

public class SmokeTest() : PlaywrightTestBase("https://playwright.dev/", Options)
{
    private static TestOptions Options => new()
    {
        Environment = "local",
        Browser = "chromium",
        Headless = true,
        Video = TestVideoOptions.Default
    };

    [Test]
    public async Task AnonymousSession_LoadsHomePage()
    {
        var session = await CreateSessionAsync();

        await Assertions.Expect(session.Page).ToHaveTitleAsync(new System.Text.RegularExpressions.Regex("Playwright"));
    }

    [Test]
    public async Task ButtonComponent_ReadsGetStartedLink()
    {
        var session = await CreateSessionAsync();

        var getStarted = new ButtonComponent(
            session.Page.GetByRole(AriaRole.Link, new() { Name = "Get started" }));

        Assert.That(await getStarted.IsVisibleAsync(), Is.True);
        await getStarted.ClickAsync();

        await Assertions.Expect(session.Page.GetByRole(AriaRole.Heading, new() { Name = "Installation" }))
            .ToBeVisibleAsync();
    }
}
