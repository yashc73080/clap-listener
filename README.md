# clap-listener

A lightweight C#/.NET Worker Service that listens to the system microphone and triggers an action when it detects a **double clap**.

## Project structure

```text
clap-listener/
├── Models/
│   ├── AudioFrame.cs
│   └── ClapEvent.cs
├── Options/
│   ├── ActionOptions.cs
│   ├── AudioCaptureOptions.cs
│   ├── ClapDetectionOptions.cs
│   └── DoubleClapOptions.cs
├── Services/
│   ├── IActionExecutor.cs
│   ├── IAudioCapture.cs
│   ├── IClapDetector.cs
│   ├── IDoubleClapDetector.cs
│   ├── ClapDetector.cs
│   ├── DoubleClapDetector.cs
│   ├── NAudioCaptureService.cs
│   └── ProcessActionExecutor.cs
├── Program.cs
├── Worker.cs
├── appsettings.json
└── clap-listener.csproj
```

## Architecture overview

- **AudioCapture** (`NAudioCaptureService`)
  - Uses WASAPI via NAudio.
  - Captures microphone input with `DataAvailable` events (non-blocking, event-driven).
  - Emits normalized `AudioFrame` objects.

- **ClapDetection** (`ClapDetector`)
  - Detects short impulse sounds.
  - Filters by amplitude threshold and impulse duration (<30 ms by default).
  - Optionally checks spectral spread using FFT to reject voice/yells.

- **DoubleClap logic** (`DoubleClapDetector`)
  - Detects two claps in a 300–600 ms window.
  - Resets when out of range.
  - Applies cooldown to avoid repeated triggers.

- **ActionExecution** (`ProcessActionExecutor`)
  - Executes a configurable process with `Process.Start`.
  - Defaults to launching Visual Studio Code (`code`).

- **Worker orchestration** (`Worker`)
  - Wires event pipeline: Audio -> Clap -> DoubleClap -> Action.
  - Starts/stops capture cleanly.

## Local run instructions

1. Install prerequisites:
   - .NET 10 SDK
   - Visual Studio Code CLI (`code`) on PATH (or change `Action:ExecutablePath`)
2. Restore and run:

```bash
dotnet restore
dotnet run
```

3. Clap twice with roughly a half-second gap.
4. Watch logs in terminal for:
   - first clap registered
   - double clap detected
   - action executed

## Configuration

Edit `appsettings.json`:

- `AudioCapture.BufferMilliseconds`: lower = lower latency, slightly higher CPU
- `ClapDetection.AmplitudeThreshold`: clap intensity threshold
- `ClapDetection.MaxImpulseDurationMs`: reject long sounds
- `ClapDetection.EnableSpectralSpreadCheck`: true/false
- `DoubleClap.MinGapMs` and `MaxGapMs`: double-clap timing window
- `Action.ExecutablePath`: executable to run when triggered

## Future extension hooks

The componentized architecture allows adding:

- snap detection and knock detection as additional detectors
- configurable hotkeys / alternative actions
- system tray integration in a separate optional module
- Windows Service hosting (`sc create` / `UseWindowsService`) for startup automation
