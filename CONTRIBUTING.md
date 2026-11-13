# Contributing

Thanks for your interest in Velvet Pointer.

## Before opening an issue

Include the Windows version/build, mouse model and DPI, polling rate if known, monitor layout/scaling, Windows pointer settings, Velvet Pointer preset/settings, and reproducible steps. Never include private information or device serial numbers.

## Development guidelines

- Keep the movement-processing path allocation-light.
- Do not add a polling loop that repeatedly calls `SetCursorPos`.
- Avoid target snapping, angle locking, or automatic cursor steering.
- Preserve the emergency-disable path.
- Treat hook/reinjection and event-ordering changes as high risk.
- Explain how changes were tested on Windows hardware.

## Pull requests

1. Describe the behavior change and its motivation.
2. Document any input-latency or pointer-feel tradeoffs.
3. Include build results and a manual test plan.
4. Call out untested scenarios, especially high polling rates, multiple mice, mixed-DPI monitors, elevated apps, and remote sessions.

A successful build is not a substitute for real-device input testing.
