using clap_listener.Models;

namespace clap_listener.Services;

public interface IActionExecutor
{
    Task ExecuteAsync(ClapEvent triggerEvent, CancellationToken cancellationToken);
}
