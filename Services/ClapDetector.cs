using clap_listener.Models;
using clap_listener.Options;
using Microsoft.Extensions.Options;
using NAudio.Dsp;

namespace clap_listener.Services;

/// <summary>
/// Identifies clap-like impulses by combining amplitude threshold, impulse duration,
/// and optional spectral spread checks.
/// </summary>
public sealed class ClapDetector : IClapDetector
{
    private readonly ClapDetectionOptions _options;
    private readonly ILogger<ClapDetector> _logger;

    public ClapDetector(IOptions<ClapDetectionOptions> options, ILogger<ClapDetector> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public event EventHandler<ClapEvent>? ClapDetected;

    public void ProcessAudioFrame(object? sender, AudioFrame frame)
    {
        var (peak, impulseSamples) = FindImpulse(frame.Samples, _options.AmplitudeThreshold);
        if (impulseSamples == 0)
        {
            return;
        }

        var durationMs = impulseSamples * 1000.0 / frame.SampleRate;
        if (durationMs < _options.MinImpulseDurationMs || durationMs > _options.MaxImpulseDurationMs)
        {
            return;
        }

        var spectralSpread = _options.EnableSpectralSpreadCheck
            ? CalculateSpectralSpread(frame.Samples, frame.SampleRate)
            : double.MaxValue;

        if (_options.EnableSpectralSpreadCheck && spectralSpread < _options.MinSpectralSpread)
        {
            return;
        }

        var clap = new ClapEvent(frame.CapturedAt, peak, durationMs, spectralSpread);
        _logger.LogDebug(
            "Clap candidate accepted. Peak={Peak:F2}, DurationMs={Duration:F2}, Spread={Spread:F0}",
            peak,
            durationMs,
            spectralSpread);
        ClapDetected?.Invoke(this, clap);
    }

    private static (float Peak, int ImpulseSamples) FindImpulse(float[] samples, float threshold)
    {
        float peak = 0;
        var longestRun = 0;
        var run = 0;

        foreach (var value in samples)
        {
            var amplitude = Math.Abs(value);
            if (amplitude >= threshold)
            {
                run++;
                if (amplitude > peak)
                {
                    peak = amplitude;
                }

                if (run > longestRun)
                {
                    longestRun = run;
                }
            }
            else
            {
                run = 0;
            }
        }

        return longestRun == 0 ? (0, 0) : (peak, longestRun);
    }

    private static double CalculateSpectralSpread(float[] samples, int sampleRate)
    {
        var fftLength = 1024;
        var complex = new Complex[fftLength];

        for (var i = 0; i < fftLength; i++)
        {
            var sample = i < samples.Length ? samples[i] : 0f;
            complex[i].X = sample;
            complex[i].Y = 0;
        }

        FastFourierTransform.FFT(true, (int)Math.Log2(fftLength), complex);

        double weightedFrequencySum = 0;
        double magnitudeSum = 0;
        for (var i = 1; i < fftLength / 2; i++)
        {
            var magnitude = Math.Sqrt((complex[i].X * complex[i].X) + (complex[i].Y * complex[i].Y));
            var frequency = i * sampleRate / (double)fftLength;
            weightedFrequencySum += frequency * magnitude;
            magnitudeSum += magnitude;
        }

        if (magnitudeSum <= double.Epsilon)
        {
            return 0;
        }

        var centroid = weightedFrequencySum / magnitudeSum;

        double spreadNumerator = 0;
        for (var i = 1; i < fftLength / 2; i++)
        {
            var magnitude = Math.Sqrt((complex[i].X * complex[i].X) + (complex[i].Y * complex[i].Y));
            var frequency = i * sampleRate / (double)fftLength;
            spreadNumerator += magnitude * Math.Pow(frequency - centroid, 2);
        }

        return Math.Sqrt(spreadNumerator / magnitudeSum);
    }
}
