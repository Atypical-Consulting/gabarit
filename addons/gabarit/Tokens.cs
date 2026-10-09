using Godot;

namespace Gabarit;

/// <summary>
/// Every visual decision of the library, as one resource a consumer can save as .tres,
/// edit in the inspector and pass to <see cref="ThemeBuilder.Build"/>.
/// </summary>
[GlobalClass]
public partial class Tokens : Resource
{
    [ExportGroup("Surfaces")]
    /// <summary>The application ground, behind sheets and panels.</summary>
    [Export] public Color Ground { get; set; } = new("#f2f2f0");
    /// <summary>The sheet: controls, tables and panels sit on it.</summary>
    [Export] public Color Paper { get; set; } = new("#ffffff");
    /// <summary>Header bands and subtle fills.</summary>
    [Export] public Color Band { get; set; } = new("#e9e9e6");

    [ExportGroup("Ink")]
    [Export] public Color Ink { get; set; } = new("#1b1c1e");
    [Export] public Color InkMuted { get; set; } = new("#55585c");
    /// <summary>Control outlines. Must keep 3:1 against Paper (WCAG 1.4.11).</summary>
    [Export] public Color Rule { get; set; } = new("#7f8287");
    /// <summary>Internal table rules; decorative, no contrast requirement.</summary>
    [Export] public Color Hairline { get; set; } = new("#d6d6d2");

    [ExportGroup("Signal")]
    /// <summary>Reserved for things that can be acted on, and for selection.</summary>
    [Export] public Color Action { get; set; } = new("#0b5cad");
    [Export] public Color OnAction { get; set; } = new("#ffffff");
    [Export] public Color ActionTint { get; set; } = new("#dce7f4");
    [Export] public Color Danger { get; set; } = new("#b3261e");

    [ExportGroup("Type")]
    [Export] public Font? Regular { get; set; }
    [Export] public Font? Bold { get; set; }
    [Export] public int FontSize { get; set; } = 14;
    [Export] public int SmallSize { get; set; } = 12;
    [Export] public int TitleSize { get; set; } = 30;

    [ExportGroup("Metrics")]
    /// <summary>Height of buttons, fields and selects.</summary>
    [Export] public int ControlHeight { get; set; } = 30;
    [Export] public int RowHeight { get; set; } = 26;
    /// <summary>The 4px grid every spacing is a multiple of.</summary>
    [Export] public int Unit { get; set; } = 4;
    [Export] public int Radius { get; set; } = 2;

    const string FontDir = "res://addons/gabarit/fonts/";

    /// <summary>White paper, black ink: the default sheet.</summary>
    public static Tokens CreatePaper() => WithFonts(new Tokens());

    /// <summary>The same sheet inverted, for dim rooms. Same token names, same contrast rules.</summary>
    public static Tokens CreateSlate() => WithFonts(new Tokens
    {
        Ground = new("#131416"),
        Paper = new("#1c1e21"),
        Band = new("#25282c"),
        Ink = new("#ecebe6"),
        InkMuted = new("#a5a8ad"),
        Rule = new("#7c8086"),
        Hairline = new("#34373c"),
        Action = new("#7ab0f0"),
        OnAction = new("#0d1520"),
        ActionTint = new("#22354d"),
        Danger = new("#f08a80"),
    });

    static Tokens WithFonts(Tokens t)
    {
        t.Regular = Tabular("GabaritFigures-Regular.otf", "D-DIN.otf");
        t.Bold = Tabular("GabaritFigures-Bold.otf", "D-DIN-Bold.otf");
        return t;
    }

    /// <summary>
    /// D-DIN has proportional digits only. A digits-only face with one shared advance
    /// (tools/make_figures.py) goes first; every other glyph falls back to D-DIN.
    /// </summary>
    static Font Tabular(string figures, string text) => new FontVariation
    {
        BaseFont = GD.Load<Font>(FontDir + figures),
        Fallbacks = new Godot.Collections.Array<Font> { GD.Load<Font>(FontDir + text) },
    };
}
