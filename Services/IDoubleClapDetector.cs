using clap_listener.Models;

namespace clap_listener.Services;

public interface IDoubleClapDetector
{
    event EventHandler<ClapEvent>? DoubleClapDetected;
    void ProcessClap(object? sender, ClapEvent clapEvent);
}
