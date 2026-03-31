namespace clap_listener.Models;

/// <summary>
/// Immutable audio payload dispatched by the capture component.
/// </summary>
public sealed record AudioFrame(float[] Samples, int SampleRate, int Channels, DateTimeOffset CapturedAt);
