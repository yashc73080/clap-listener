using clap_listener.Models;
using clap_listener.Services;

namespace clap_listener;

public sealed class Worker : BackgroundService
{
    private readonly IAudioCapture _audioCapture;
    private readonly IClapDetector _clapDetector;
    private readonly IDoubleClapDetector _doubleClapDetector;
    private readonly IActionExecutor _actionExecutor;
    private readonly ILogger<Worker> _logger;

    public Worker(
        IAudioCapture audioCapture,
        IClapDetector clapDetector,
        IDoubleClapDetector doubleClapDetector,
        IActionExecutor actionExecutor,
        ILogger<Worker> logger)
    {
        _audioCapture = audioCapture;
        _clapDetector = clapDetector;
        _doubleClapDetector = doubleClapDetector;
        _actionExecutor = actionExecutor;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _audioCapture.FrameCaptured += _clapDetector.ProcessAudioFrame;
        _clapDetector.ClapDetected += _doubleClapDetector.ProcessClap;
        _doubleClapDetector.DoubleClapDetected += OnDoubleClapDetected;

        _logger.LogInformation("Clap listener worker started.");
        await _audioCapture.StartAsync(stoppingToken);

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // Expected when service stops.
        }
        finally
        {
            _doubleClapDetector.DoubleClapDetected -= OnDoubleClapDetected;
            _clapDetector.ClapDetected -= _doubleClapDetector.ProcessClap;
            _audioCapture.FrameCaptured -= _clapDetector.ProcessAudioFrame;

            await _audioCapture.StopAsync(CancellationToken.None);
            await _audioCapture.DisposeAsync();
            _logger.LogInformation("Clap listener worker stopped.");
        }
    }

    private void OnDoubleClapDetected(object? sender, ClapEvent clapEvent)
    {
        _ = Task.Run(() => _actionExecutor.ExecuteAsync(clapEvent, CancellationToken.None));
    }
}
