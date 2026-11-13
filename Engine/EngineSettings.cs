namespace VelvetPointer.Engine;
public sealed class EngineSettings
{
    public double Smoothing { get; set; } = 28;
    public double JitterReduction { get; set; } = 24;
    public double CurveRefinement { get; set; } = 20;
    public double LineRefinement { get; set; } = 18;
    public double Responsiveness { get; set; } = 82;
    public double InputScale { get; set; } = 1.0;
    public bool Adaptive { get; set; } = true;
    public bool Enabled { get; set; } = false;
    public string Preset { get; set; } = "macOS";
}
