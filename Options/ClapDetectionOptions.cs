namespace clap_listener.Options;

public sealed class ClapDetectionOptions
{
    public const string SectionName = "ClapDetection";

    public float AmplitudeThreshold { get; set; } = 0.45f;
    public double MaxImpulseDurationMs { get; set; } = 30;
    public double MinImpulseDurationMs { get; set; } = 3;
    public bool EnableSpectralSpreadCheck { get; set; } = true;
    public double MinSpectralSpread { get; set; } = 1200;
}
