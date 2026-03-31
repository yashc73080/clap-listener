namespace clap_listener.Options;

public sealed class AudioCaptureOptions
{
    public const string SectionName = "AudioCapture";

    /// <summary>
    /// WASAPI capture latency. Smaller values reduce delay but can increase CPU usage.
    /// </summary>
    public int BufferMilliseconds { get; set; } = 30;

    /// <summary>
    /// Optional microphone device name match; first input device is used when empty.
    /// </summary>
    public string? PreferredDeviceName { get; set; }
}
