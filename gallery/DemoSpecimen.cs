using Godot;

namespace Gabarit.Gallery;

/// <summary>The object the PropertyInspector specimen edits: an export preset, synthetic.</summary>
public partial class DemoSpecimen : Resource
{
    public enum Format { Png, WebP, Jpeg }

    [ExportGroup("Output")]
    [Export] public string PresetName { get; set; } = "Release — desktop";
    [Export] public Format ImageFormat { get; set; } = Format.WebP;
    [Export(PropertyHint.Range, "1,100,1")] public int Quality { get; set; } = 85;
    [Export] public bool StripMetadata { get; set; } = true;

    [ExportGroup("Layout")]
    [Export(PropertyHint.Range, "0.5,4,0.25")] public float UiScale { get; set; } = 1.25f;
    [Export] public int ColumnCount { get; set; } = 3;
    [Export] public Color AccentColor { get; set; } = new("#0b5cad");
}
