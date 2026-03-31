using clap_listener.Models;
using clap_listener.Options;
using Microsoft.Extensions.Options;

namespace clap_listener.Services;

/// <summary>
/// State machine that turns valid clap events into a debounced double-clap trigger.
/// </summary>
public sealed class DoubleClapDetector : IDoubleClapDetector
{
    private readonly DoubleClapOptions _options;
    private readonly ILogger<DoubleClapDetector> _logger;
    private readonly object _gate = new();

    private DateTimeOffset? _lastClapAt;
    private DateTimeOffset _lastTriggerAt = DateTimeOffset.MinValue;

    public DoubleClapDetector(IOptions<DoubleClapOptions> options, ILogger<DoubleClapDetector> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public event EventHandler<ClapEvent>? DoubleClapDetected;

    public void ProcessClap(object? sender, ClapEvent clapEvent)
    {
        lock (_gate)
        {
            if ((clapEvent.DetectedAt - _lastTriggerAt).TotalMilliseconds < _options.TriggerCooldownMs)
            {
                return;
            }

            if (_lastClapAt is null)
            {
                _lastClapAt = clapEvent.DetectedAt;
                _logger.LogInformation("First clap registered at {DetectedAt}", clapEvent.DetectedAt);
                return;
            }

            var gapMs = (clapEvent.DetectedAt - _lastClapAt.Value).TotalMilliseconds;
            if (gapMs >= _options.MinGapMs && gapMs <= _options.MaxGapMs)
            {
                _lastTriggerAt = clapEvent.DetectedAt;
                _lastClapAt = null;
                _logger.LogInformation("Double clap detected with gap {GapMs:F0} ms", gapMs);
                DoubleClapDetected?.Invoke(this, clapEvent);
                return;
            }

            _logger.LogDebug("Clap gap {GapMs:F0} ms outside range. Resetting window.", gapMs);
            _lastClapAt = clapEvent.DetectedAt;
        }
    }
}
