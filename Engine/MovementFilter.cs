using System.Diagnostics;
namespace VelvetPointer.Engine;

/// <summary>Low-allocation direction smoother. Each event's displacement magnitude is preserved
/// (apart from the deliberately configured micro-jitter attenuation), so the filter does not
/// accumulate positional lag or keep moving after the hand stops.</summary>
public sealed class MovementFilter
{
    private double _dirX, _dirY, _fractionX, _fractionY;
    private bool _hasDirection;
    private long _lastTicks;
    public double LastIntensity { get; private set; }
    public double LastSpeed { get; private set; }

    public (int X, int Y) Process(int dx, int dy, EngineSettings s)
    {
        long now = Stopwatch.GetTimestamp();
        double dt = _lastTicks == 0 ? 0.008 : Math.Clamp((now - _lastTicks) / (double)Stopwatch.Frequency, 0.001, 0.05);
        _lastTicks = now;
        double x = dx * Math.Clamp(s.InputScale, 0.1, 5.0), y = dy * Math.Clamp(s.InputScale, 0.1, 5.0);
        double magnitude = Math.Sqrt(x*x + y*y);
        double speed = magnitude / dt;
        LastSpeed = speed;
        if (magnitude < 1e-6) return (0, 0);

        // A continuous soft dead zone, rather than a hard cutoff, attenuates only the smallest
        // samples. Its threshold is less aggressive for adaptive mode to retain fine control.
        double dead = (s.JitterReduction / 100.0) * (s.Adaptive ? 0.65 : 0.40);
        double gain = dead <= 0 ? 1.0 : magnitude / (magnitude + dead);
        magnitude *= gain;
        double ux = x / Math.Sqrt(x*x + y*y), uy = y / Math.Sqrt(x*x + y*y);

        double smooth = Math.Clamp(s.Smoothing / 100.0, 0, 0.85);
        double response = Math.Clamp(s.Responsiveness / 100.0, 0.15, 1.0);
        double speedFactor = s.Adaptive ? Math.Clamp(speed / 1800.0, 0.08, 1.0) : 1.0;
        double directionBlend = smooth * (1.0 - speedFactor) * (1.0 - response * 0.45);
        directionBlend *= 1.0 - Math.Clamp((s.CurveRefinement + s.LineRefinement) / 250.0, 0, 0.55);
        directionBlend = Math.Clamp(directionBlend, 0, 0.78);

        if (_hasDirection && directionBlend > 0)
        {
            double bx = ux * (1.0 - directionBlend) + _dirX * directionBlend;
            double by = uy * (1.0 - directionBlend) + _dirY * directionBlend;
            double norm = Math.Sqrt(bx*bx + by*by);
            if (norm > 1e-6) { ux = bx / norm; uy = by / norm; }
        }
        _dirX = ux; _dirY = uy; _hasDirection = true;
        LastIntensity = directionBlend * 100.0;

        // Preserve fractional pixels across events; no SetCursorPos polling or delayed tail.
        _fractionX += ux * magnitude; _fractionY += uy * magnitude;
        int outX = (int)Math.Truncate(_fractionX), outY = (int)Math.Truncate(_fractionY);
        _fractionX -= outX; _fractionY -= outY;
        return (outX, outY);
    }

    public void Reset() { _dirX = _dirY = _fractionX = _fractionY = 0; _hasDirection = false; _lastTicks = 0; LastIntensity = LastSpeed = 0; }
}
