using Microsoft.Playwright;

namespace PlaywrightLibrary.Testing;

public record TestVideoOptions
{
    private static RecordVideoSize DefaultSize => new() { Width = 1280, Height = 720 };

    public static TestVideoOptions Default => new();

    public string Directory { get; init; } = "test-failures";
    public RecordVideoSize Size { get; init; } = DefaultSize;
}
