using clap_listener.Models;

namespace clap_listener.Services;

public interface IAudioCapture : IAsyncDisposable
{
    event EventHandler<AudioFrame>? FrameCaptured;
    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
}
