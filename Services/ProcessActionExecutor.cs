using System.Diagnostics;
using clap_listener.Models;
using clap_listener.Options;
using Microsoft.Extensions.Options;

namespace clap_listener.Services;

/// <summary>
/// Executes configured process actions when a trigger is fired.
/// </summary>
public sealed class ProcessActionExecutor : IActionExecutor
{
    private readonly ActionOptions _options;
    private readonly ILogger<ProcessActionExecutor> _logger;

    public ProcessActionExecutor(IOptions<ActionOptions> options, ILogger<ProcessActionExecutor> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public Task ExecuteAsync(ClapEvent triggerEvent, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var startInfo = new ProcessStartInfo
        {
            FileName = _options.ExecutablePath,
            Arguments = _options.Arguments,
            UseShellExecute = true,
            ErrorDialog = false
        };

        try
        {
            Process.Start(startInfo);
            _logger.LogInformation(
                "Action executed for double clap at {DetectedAt}. Command={Command} {Arguments}",
                triggerEvent.DetectedAt,
                startInfo.FileName,
                startInfo.Arguments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to launch configured executable: {ExecutablePath}", _options.ExecutablePath);
        }

        return Task.CompletedTask;
    }
}
