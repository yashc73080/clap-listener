namespace clap_listener.Options;

public sealed class ActionOptions
{
    public const string SectionName = "Action";

    public string ExecutablePath { get; set; } = "code";
    public string? Arguments { get; set; }
}
