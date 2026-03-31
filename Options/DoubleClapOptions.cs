namespace clap_listener.Options;

public sealed class DoubleClapOptions
{
    public const string SectionName = "DoubleClap";

    public int MinGapMs { get; set; } = 300;
    public int MaxGapMs { get; set; } = 600;
    public int TriggerCooldownMs { get; set; } = 1200;
}
