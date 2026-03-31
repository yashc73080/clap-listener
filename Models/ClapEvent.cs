namespace clap_listener.Models;

/// <summary>
/// Represents a validated clap impulse.
/// </summary>
public sealed record ClapEvent(DateTimeOffset DetectedAt, float PeakAmplitude, double DurationMs, double SpectralSpread);
