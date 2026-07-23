namespace PlaywrightLibrary.Testing;

public record TestOptions
{
    public required string Environment { get; init; }
    public required string Browser { get; init; }
    public bool Headless { get; init; } = true;
    public float? SlowMo { get; init; } = null;
    public TestVideoOptions? Video { get; init; } = null;
    public TestTraceOptions? Trace { get; init; } = null;
}
