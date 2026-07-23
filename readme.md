# CSharp-Playwright-Library

[![UI Tests](https://github.com/JamesHulsey/CSharp-Playwright-Library/actions/workflows/ci.yml/badge.svg)](https://github.com/JamesHulsey/CSharp-Playwright-Library/actions/workflows/ci.yml)

Standalone Playwright test framework: NUnit lifecycle base, session management, storage-state auth caching, video/screenshot on failure, and a minimal component model.

## Layout

```
CSharp-Playwright-Library.slnx
src/PlaywrightLibrary/
  Testing/        PlaywrightTestBase, PlaywrightSession, TestOptions,
                  TestVideoOptions, PlaywrightAuthOptions, PlaywrightAuthHelper,
                  TestMediaHelper
  Components/     IComponent, ButtonComponent, TextInput, CheckboxInput
  Extensions/     LocatorExtensions
tests/PlaywrightLibrary.SmokeTests/
  GlobalSetup.cs   installs the browser once before any test runs
  SmokeTest.cs     minimal check against playwright.dev that the harness boots
samples/TodoApp.UiTests/
  Infrastructure/
    TestConfig.cs            reads run config from the .runsettings parameters
    TodoAppTestBase.cs       project base: wires the library base to TestConfig
  Model/
    TodoItem.cs              test-data record passed around instead of raw strings
  Pages/
    TodoPage.cs              page object; hands out TodoRow component objects
    TodoRow.cs               component object for a single todo row
  Tests/
    TodoManagementTests.cs   add / complete / clear behaviors
    SessionIsolationTests.cs multi-user context isolation
  GlobalSetup.cs             the consumer's own browser-install fixture
  TodoApp.UiTests.runsettings  URL, browser, headless, slowMo, environment
```

`samples/TodoApp.UiTests` is a standalone project that consumes the library
exactly as a downstream team would — through a project reference to its public
API, with no direct dependency on Playwright itself. It's laid out like a real
automation suite:

- **`TestConfig`** reads the run settings (target URL, browser, headless, slowMo,
  environment) from the `<TestRunParameters>` in `TodoApp.UiTests.runsettings`,
  each with a code-level default fallback. Edit that file to run headed, switch
  browser, or point at another environment — no code change, no machine
  environment variables. The csproj wires the file in via `RunSettingsFilePath`,
  so `dotnet test` applies it automatically.
- **`TodoAppTestBase`** extends the library's `PlaywrightTestBase`, binds it to
  `TestConfig`, and exposes `OpenTodoAppAsync()` — one place for tests to get a
  ready page object.
- **`TodoPage`** is a page object: it owns the page-level locators and actions
  (`AddTodoAsync`, `ClearCompletedAsync`) and hands out **`TodoRow`** component
  objects via `Row(todo)` / `GetRowsAsync()`.
- **`TodoRow`** models a single row (like a table row): it wraps one row's locator
  and resolves its title and checkbox relative to that root, exposing
  `CompleteAsync`, `IsCompletedAsync`, `IsStruckThroughAsync`. It composes the
  library's `CheckboxInput` and locator extensions.
- **`TodoItem`** is a small record used as test data — tests pass todos around as
  typed objects instead of bare strings.
- **Tests** stay thin and readable, talking only to the page and row objects, and
  use web-first assertions.

> **On the dependency style:** this sample uses a **project reference** to keep the
> repo clone-and-run simple. In a production setting the preferred approach is to
> publish the library as a versioned **NuGet package** and consume it with
> `dotnet add package` — that's what you'd reach for once the library has its own
> release cadence and is shared across repositories. It's deliberately left out
> here to avoid the packaging/feed setup a single example doesn't need.

## Getting started

```bash
dotnet test
```

That's it. Each test project's `PlaywrightBrowserSetup` fixture (`GlobalSetup.cs`)
downloads the Chromium browser on the first run — no manual `playwright install`
step. The download is cached per user, so subsequent runs start immediately.

> On Linux, the browser binary also needs OS-level libraries to launch. CI
> installs those with `playwright.ps1 install --with-deps` (see below); locally,
> run the same command once if a browser fails to start.

## Continuous integration

`.github/workflows/ci.yml` runs the `TodoApp.UiTests` suite on every merge to
`main` (a merge lands as a push), on a GitHub-hosted Ubuntu runner. It builds the
UI project — which pulls in the library via its project reference — installs
Chromium with `--with-deps` so it can launch headless, and runs only the UI tests,
which exercise the library end to end.

## Writing a test

```csharp
public class MyTest() : PlaywrightTestBase("https://my-app.example.com", Options)
{
    private static TestOptions Options => new()
    {
        Environment = "qa",
        Browser = "chromium",
        Headless = true,
        Video = TestVideoOptions.Default
    };

    [Test]
    public async Task DoesSomething()
    {
        var session = await CreateSessionAsync();          // anonymous
        // var session = await CreateSessionAsync(authOptions);  // authenticated

        var save = new ButtonComponent(session.Page.GetByRole(AriaRole.Button, new() { Name = "Save" }));
        await save.ClickAsync();
    }
}
```

Multi-user tests: call `CreateSessionAsync` more than once. Every session is tracked and disposed in teardown.

## Authentication

```csharp
private static PlaywrightAuthOptions AdminAuth => new()
{
    AuthFilePath = "auth-state.admin.json",
    LoginAction = async page =>
    {
        await page.GetByLabel("Username").FillAsync("...");
        await page.GetByLabel("Password").FillAsync("...");
        await page.GetByRole(AriaRole.Button, new() { Name = "Sign in" }).ClickAsync();
        return await page.GetByTestId("home").IsVisibleAsync();
    }
};
```

The first call opens a headed browser, runs `LoginAction`, and caches storage state to `AuthFilePath`.
Subsequent tests reuse the cache until `CacheLifetime` (default 12h) elapses. Locking is per file
path, so multiple roles can be minted concurrently.

## Media & traces

- **Video** records for every session when `TestOptions.Video` is set.
- A **Playwright trace** records for every session when `TestOptions.Trace` is set
  (retain-on-failure style — the richest debugging artifact Playwright offers).
- On pass: video and trace are discarded and empty directories pruned.
- On fail: the video, a full-page **screenshot**, and the **trace zip** are kept
  together and attached to the NUnit test result, so they surface in test reports
  and CI. Open a trace with `playwright show-trace trace.zip`.
- Output path: `{Directory}/{yyyy-MM-dd}/{Environment}/{TestName}/{HH.mm.ss}/`
- Directories older than one day are cleaned once per run.

Set `Video`/`Trace` to `null` to disable either.

## Parallelism

`PlaywrightTestBase` is marked `[Parallelizable(ParallelScope.All)]` with
`[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]` — a fresh fixture instance per test, each with
its own browser context, so instance fields are safe without any locking. The browser itself is
shared across tests (see [Design decisions](#design-decisions)). Worker count is set in `.runsettings`.

## Scope

The component model is deliberately small — `ButtonComponent`, `TextInput`, and
`CheckboxInput` — each a thin wrapper that gives a locator an intent-revealing API
rather than trying to model every possible widget.
Anything more specialized (tables, dropdowns, date pickers, auto-complete) is
left to the consuming project, which knows its own DOM.

The library stays application-agnostic: it assumes only standard ARIA roles and
test IDs, so it drops into any Playwright + NUnit project. `samples/TodoApp.UiTests`
is a working example of exactly that.

## Design decisions

The trade-offs worth knowing, and why they were made:

- **One shared browser, a context per test.** Launching a browser is expensive;
  contexts are cheap and fully isolated. `SharedBrowser` launches one browser per
  launch-config and reuses it, while each test gets its own context. Browsers are
  disposed on process exit, because a library has no assembly-level teardown hook
  inside the consumer.
- **Config is the consumer's job, not the library's.** `PlaywrightTestBase` takes
  `TestOptions` rather than inventing them, and `Browser`/`Environment` are
  `required` to force a conscious choice. The library is the mechanism; the consumer
  owns policy — `TodoApp.UiTests` sources that policy from `.runsettings`.
- **Run settings over environment variables.** The sample reads config from
  `<TestRunParameters>` — a file people edit — rather than machine environment
  variables. Cleaner precedence, and `.runsettings` stays reserved for runner
  concerns while app config rides in one committed file.
- **Project reference over a NuGet package.** The sample consumes the library by
  project reference to keep the repo clone-and-run simple; production would publish
  a versioned package. Left deliberate rather than hidden.
- **No `networkidle` wait.** Playwright discourages it; readiness is asserted with
  web-first assertions instead, which are less flaky.
- **Retain-on-failure artifacts.** Video, trace, and screenshot are captured but
  only kept — and attached to the test result — when a test fails, so green runs
  stay clean.
