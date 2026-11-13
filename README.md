# Velvet Pointer

**A native Windows mouse-movement refinement prototype focused on smoothness, precision, and responsive pointer control.**

Velvet Pointer explores whether a lightweight user-mode Windows application can make ordinary mouse movement feel more consistent without steering the pointer toward targets, snapping movement to fixed angles, or adding a delayed smoothing tail.

> [!WARNING]
> **Experimental software — use at your own risk.** Velvet Pointer installs a low-level mouse hook and can suppress physical mouse-move events while enhancement is enabled. It has not yet been validated across a representative range of Windows systems and mouse hardware. Start with enhancement disabled, keep the emergency shortcut available, and do not use this prototype in safety-critical workflows.

## Project status

| Area | Status |
|---|---|
| Native Windows UI | Implemented (WPF) |
| Raw Input capture | Implemented |
| Low-level mouse interception | Implemented |
| Filter and reinjection path | Implemented; hardware validation required |
| Presets and tuning controls | Implemented in the UI |
| Live trajectory display | Implemented in the UI |
| System tray and emergency bypass | Implemented |
| Automated Windows build | GitHub Actions workflow included |
| Signed driver / virtual HID | Not implemented |
| End-to-end latency certification | Not measured |

The source has **not been compiled or hardware-tested as part of this delivery**. The CI workflow is intended to perform the first reproducible Windows build after the project is pushed to GitHub.

## Goals

- Reduce distracting micro-jitter while preserving intentional movement.
- Refine local direction changes without snapping to predefined angles.
- Keep fast movements responsive and avoid a trailing cursor effect.
- Offer understandable controls and presets rather than hidden pointer manipulation.
- Keep a global emergency bypass available.

Velvet Pointer does not aim for pixel-perfect geometric circles or automatic target acquisition. The current filter is a conservative direction-coherence filter, not a trajectory prediction or target-snapping system.

## Features

### Movement engine

- **Adaptive direction blending:** blends the current movement direction with a recent direction estimate. The blend is reduced as movement speed rises.
- **Soft micro-movement attenuation:** applies a continuous gain to very small input samples rather than a hard dead zone.
- **Fractional movement accumulation:** carries sub-pixel fractions between events to avoid simply discarding every fractional result.
- **Responsiveness control:** reduces the amount of directional blending as responsiveness is increased.
- **No polling-based cursor reposition loop:** the implementation does not repeatedly call `SetCursorPos` to chase the pointer.

### Presets and controls

The interface provides macOS, Precision, Natural, Robotic, and Custom preset choices, along with controls for smoothing, jitter reduction, curve refinement, straight-line refinement, responsiveness, adaptive processing, and raw-input scale.

The actual feel depends on the mouse, its DPI, Windows pointer settings, polling rate, and the selected raw-input scale. Treat presets as starting points, not universal calibrations.

### Live interface

The WPF interface includes Overview, Movement Engine, Precision Settings, Presets, Live Visualisation, and Advanced Settings sections. The live display is intended to compare the captured input trajectory with the refined output and expose engine statistics.

## How it works

The application combines two Windows input mechanisms:

1. **Raw Input** (`RegisterRawInputDevices` / `WM_INPUT`) captures mouse-device movement deltas.
2. **A low-level mouse hook** (`WH_MOUSE_LL`) observes physical mouse-move messages and, when enabled, suppresses those physical move messages while the replacement movement is injected through `SendInput`.

Injected events are identified using the low-level hook's injected-event flag and passed through, preventing the app from processing its own reinjected movement again.

The output path uses virtual-desktop coordinates so it can account for the bounding rectangle spanning multiple monitors. This is a user-mode architecture; it is not a signed HID or kernel filter driver.

### Important input-model limitation

Raw Input reports device counts. It does **not** directly report the final screen-pixel displacement Windows would have produced after applying pointer speed and acceleration. The app therefore exposes a **Raw input scale** calibration setting. A value of `1.0×` means one raw count per output pixel in this implementation; that may not match the user's normal Windows pointer configuration.

Raw Input delivery and low-level hook callbacks are scheduled separately by Windows. This creates important ordering and timing questions that must be tested on real hardware, especially with high-polling-rate devices, multiple mice, elevated applications, remote sessions, and mixed-DPI monitor layouts.

The UI's processing-time statistic is not an end-to-end measurement of physical-device-to-screen latency.

## Requirements

- Windows 10 or Windows 11 (64-bit recommended)
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- A Windows environment for building and running the WPF application

The project targets `net8.0-windows`, uses WPF, and enables Windows Forms for the system-tray integration.

## Build and run

Open PowerShell in the repository root:

```powershell
dotnet --info
dotnet restore
dotnet build -c Release
dotnet run -c Release
```

### Publish a self-contained x64 executable

```powershell
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```

Published files are written to:

```text
bin/Release/net8.0-windows/win-x64/publish/
```

The executable can be launched directly from that folder. Self-contained publishing includes the .NET runtime, so a separate .NET runtime installation is not required on the target machine.

## First-run safety checklist

1. Launch Velvet Pointer with enhancement **off**.
2. Read the current Windows pointer speed and acceleration settings so you can restore them if needed.
3. Enable enhancement only when you are ready to test.
4. Start with the Natural or macOS preset and a conservative raw-input scale.
5. Test slow movement, fast movement, diagonal movement, clicks during movement, and movement across monitor boundaries.
6. If movement becomes unexpected, use **Ctrl + Alt + Pause** or the tray menu's emergency-disable command.
7. Exit the application to remove its hook if the emergency bypass is unavailable.

The emergency shortcut is **Ctrl + Alt + Pause**.

## Configuration notes

- **Smoothing:** controls the maximum directional blending contribution.
- **Jitter reduction:** controls the soft attenuation applied to very small movement samples.
- **Curve refinement / line refinement:** influence the direction-blend amount; they do not run separate geometric curve or line recognizers.
- **Responsiveness:** reduces directional blending as responsiveness increases.
- **Adaptive:** scales refinement according to estimated movement speed.
- **Raw input scale:** maps device counts into output displacement and may require per-device calibration.

Avoid setting every control to maximum. More filtering is not automatically better and can reduce the fidelity of intentional micro-adjustments.

## Repository layout

```text
.
├── .github/
│   └── workflows/
│       └── build.yml          # Windows CI build and publish artifact
├── Engine/
│   ├── EngineSettings.cs      # Engine configuration model
│   └── MovementFilter.cs      # Low-allocation movement filter
├── Interop/
│   └── NativeMethods.cs       # Win32 interop declarations
├── Properties/
├── App.xaml                   # WPF application resources
├── App.xaml.cs
├── MainWindow.xaml            # Desktop interface
├── MainWindow.xaml.cs         # UI, hook/input integration, tray and settings
├── app.manifest
└── VelvetPointer.csproj
```

## Continuous integration

The workflow in `.github/workflows/build.yml` runs on GitHub Actions using a Windows runner. It restores the project, publishes a self-contained x64 build, and uploads the published output as a workflow artifact.

To retrieve a build:

1. Open the repository's **Actions** tab.
2. Select the latest successful **Build Windows app** run.
3. Download the `VelvetPointer-win-x64` artifact.

A successful CI build verifies compilation and publishing only. It does not replace interactive testing on physical Windows hardware.

## Troubleshooting

### The pointer feels too fast, too slow, or unlike normal Windows movement

Adjust **Raw input scale** gradually. Raw Input counts are not equivalent to post-acceleration Windows screen pixels, so the default scale may not reproduce your previous pointer speed.

### Movement is unexpectedly altered

Use **Ctrl + Alt + Pause** to disable enhancement immediately, or choose Emergency disable from the tray menu. If necessary, exit the application.

### The application does not affect a secure desktop

This is expected for a user-mode low-level hook. The hook does not operate on the secure desktop, such as the UAC consent screen.

### Behavior differs across applications

Some elevated, remote, fullscreen, or input-intensive applications may handle input differently. This prototype has not been certified for games, remote desktop sessions, or elevated application scenarios.

### A multi-monitor setup behaves incorrectly

Please report monitor arrangement, scaling percentages, display resolution, Windows version, and whether monitors use mixed DPI. These cases require hardware testing.

## Known limitations

- This is a user-mode prototype, not a signed virtual-HID device or kernel filter driver.
- Raw Input and low-level hook timing are not guaranteed to form a perfectly synchronized event stream.
- Raw device counts do not include the final Windows pointer acceleration result.
- Circle and arc geometry is not explicitly reconstructed.
- The processing statistic is not a physical end-to-end latency measurement.
- Mixed-DPI, high-polling-rate, remote-session, and elevated-window behavior needs validation.
- No claim is made that the current build is equivalent to macOS pointer acceleration or feel.

## Contributing

Bug reports and carefully documented hardware test results are especially useful. Before opening an issue, include:

- Windows edition and build
- Mouse make/model and configured DPI
- Polling rate, if known
- Monitor count, arrangement, resolution, and scaling
- Windows pointer speed and Enhance Pointer Precision setting
- Velvet Pointer preset and all relevant engine settings
- Exact reproduction steps and whether the emergency shortcut worked

Please do not include personal information, serial numbers, or sensitive system logs.

For code changes, keep the filter allocation-light and avoid introducing polling loops that repeatedly reposition the cursor. Any change to hook, reinjection, or event ordering should include an explanation of its safety implications and a Windows hardware test plan.

## Roadmap

- [ ] Compile and validate on Windows 10 and Windows 11
- [ ] Measure end-to-end latency with a repeatable external test method
- [ ] Test 125 Hz through high-polling-rate mice
- [ ] Validate click ordering and multi-device behavior
- [ ] Validate mixed-DPI multi-monitor configurations
- [ ] Add automated unit tests for movement-filter invariants
- [ ] Add user-facing diagnostics and exportable settings
- [ ] Evaluate a virtual-HID architecture if user-mode limitations prove unacceptable
- [ ] Package a signed installer after the input path is stable

## License

No license has been selected yet. Until a license is added, the repository's source is not explicitly granted open-source reuse, modification, or redistribution rights. Add a `LICENSE` file before inviting external contributions or redistribution.

## Disclaimer

Velvet Pointer is an experimental software project provided without warranty. The authors are not responsible for unexpected pointer behavior, lost work, or other damage arising from its use. Use the emergency bypass and test carefully.
