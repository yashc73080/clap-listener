using clap_listener.Models;
using clap_listener.Options;
using Microsoft.Extensions.Options;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace clap_listener.Services;

/// <summary>
/// Event-driven microphone capture using WASAPI loop with low latency.
/// </summary>
public sealed class NAudioCaptureService : IAudioCapture
{
    private readonly AudioCaptureOptions _options;
    private readonly ILogger<NAudioCaptureService> _logger;
    private readonly object _sync = new();

    private WasapiCapture? _capture;
    private bool _started;

    public NAudioCaptureService(IOptions<AudioCaptureOptions> options, ILogger<NAudioCaptureService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public event EventHandler<AudioFrame>? FrameCaptured;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        lock (_sync)
        {
            if (_started)
            {
                return Task.CompletedTask;
            }

            var device = ResolveCaptureDevice();
            _capture = new WasapiCapture(device, true, _options.BufferMilliseconds)
            {
                ShareMode = AudioClientShareMode.Shared
            };

            _capture.WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(16000, 1);
            _capture.DataAvailable += OnDataAvailable;
            _capture.RecordingStopped += OnRecordingStopped;

            _capture.StartRecording();
            _started = true;

            _logger.LogInformation(
                "Audio capture started. Device: {Device}; Rate: {Rate}; Channels: {Channels}",
                device.FriendlyName,
                _capture.WaveFormat.SampleRate,
                _capture.WaveFormat.Channels);
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        lock (_sync)
        {
            if (!_started)
            {
                return Task.CompletedTask;
            }

            _capture?.StopRecording();
            _started = false;
        }

        return Task.CompletedTask;
    }

    private MMDevice ResolveCaptureDevice()
    {
        using var enumerator = new MMDeviceEnumerator();
        var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
        if (devices.Count == 0)
        {
            throw new InvalidOperationException("No active microphone devices were found.");
        }

        if (!string.IsNullOrWhiteSpace(_options.PreferredDeviceName))
        {
            var selected = devices.FirstOrDefault(d =>
                d.FriendlyName.Contains(_options.PreferredDeviceName, StringComparison.OrdinalIgnoreCase));
            if (selected is not null)
            {
                return selected;
            }

            _logger.LogWarning("Preferred device '{Device}' not found. Falling back to default microphone.", _options.PreferredDeviceName);
        }

        return devices[0];
    }

    private void OnDataAvailable(object? sender, WaveInEventArgs e)
    {
        if (_capture is null || e.BytesRecorded == 0)
        {
            return;
        }

        // Each sample is IEEE float (4 bytes) due to WaveFormat assignment above.
        var sampleCount = e.BytesRecorded / sizeof(float);
        var samples = new float[sampleCount];
        Buffer.BlockCopy(e.Buffer, 0, samples, 0, e.BytesRecorded);

        FrameCaptured?.Invoke(this,
            new AudioFrame(samples, _capture.WaveFormat.SampleRate, _capture.WaveFormat.Channels, DateTimeOffset.UtcNow));
    }

    private void OnRecordingStopped(object? sender, StoppedEventArgs e)
    {
        if (e.Exception is not null)
        {
            _logger.LogError(e.Exception, "Microphone capture stopped with an error.");
            return;
        }

        _logger.LogInformation("Audio capture stopped.");
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync(CancellationToken.None);

        if (_capture is not null)
        {
            _capture.DataAvailable -= OnDataAvailable;
            _capture.RecordingStopped -= OnRecordingStopped;
            _capture.Dispose();
            _capture = null;
        }
    }
}
