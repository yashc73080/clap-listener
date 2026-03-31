using clap_listener.Models;

namespace clap_listener.Services;

public interface IClapDetector
{
    event EventHandler<ClapEvent>? ClapDetected;
    void ProcessAudioFrame(object? sender, AudioFrame frame);
}
