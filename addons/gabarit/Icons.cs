using System.Collections.Generic;
using Godot;

namespace Gabarit;

/// <summary>
/// The library's icon set: authored 16px SVG, one 1.5px square-capped stroke.
/// Rendered through <see cref="DpiTexture"/> so they stay sharp at any UI scale.
/// </summary>
public static class Icons
{
    const string Stroke = "fill='none' stroke='{0}' stroke-width='1.5' stroke-linecap='square' stroke-linejoin='miter'";

    static readonly Dictionary<string, string> Paths = new()
    {
        ["check"] = "<path d='M3.5 8.5 6.5 11.5 12.5 4.5'/>",
        ["chevron-down"] = "<path d='M4 6 8 10 12 6'/>",
        ["chevron-up"] = "<path d='M4 10 8 6 12 10'/>",
        ["chevron-right"] = "<path d='M6 4 10 8 6 12'/>",
        ["sort-asc"] = "<path d='M8 13V3.5M4.5 7 8 3.5 11.5 7'/>",
        ["sort-desc"] = "<path d='M8 3v9.5M4.5 9 8 12.5 11.5 9'/>",
        ["search"] = "<circle cx='7' cy='7' r='4.25'/><path d='M10.25 10.25 13.5 13.5'/>",
        ["plus"] = "<path d='M8 3.5v9M3.5 8h9'/>",
        ["minus"] = "<path d='M3.5 8h9'/>",
        ["close"] = "<path d='M4 4l8 8M12 4l-8 8'/>",
        ["filter"] = "<path d='M2.5 3.5h11L9.25 8.5v4.5L6.75 12V8.5Z'/>",
        ["folder"] = "<path d='M2 3.5h4.25l1.5 1.5H14V12.5H2Z'/>",
        ["file"] = "<path d='M3.5 1.75h6l3 3v9.5h-9Z M9.5 1.75v3h3'/>",
        ["download"] = "<path d='M8 2.5v8M4.5 7 8 10.5 11.5 7M2.5 13.5h11'/>",
        ["refresh"] = "<path d='M12.75 6.5A5 5 0 1 0 13 9.5M13 2.75V6.5H9.25'/>",
        ["settings"] = "<circle cx='8' cy='8' r='2'/><path d='M8 1.5v2M8 12.5v2M1.5 8h2M12.5 8h2M3.4 3.4l1.4 1.4M11.2 11.2l1.4 1.4M3.4 12.6l1.4-1.4M11.2 4.8l1.4-1.4'/>",
    };

    public static IEnumerable<string> Names => Paths.Keys;

    /// <summary>A named icon in the given colour.</summary>
    static readonly Dictionary<(string, Color), Texture2D> Cache = new();

    public static Texture2D Get(string name, Color color)
    {
        if (Cache.TryGetValue((name, color), out var hit)) return hit;
        var body = Paths.TryGetValue(name, out var p) ? p : Paths["close"];
        var svg = $"<svg xmlns='http://www.w3.org/2000/svg' width='16' height='16' viewBox='0 0 16 16'><g {string.Format(Stroke, Hex(color))}>{body}</g></svg>";
        return Cache[(name, color)] = FromSvg(svg);
    }

    /// <summary>Checkbox square. States are marks: empty frame, filled ink with a check, dashed when disabled.</summary>
    public static Texture2D Box(bool on, bool disabled, Tokens t)
    {
        var dash = disabled ? " stroke-dasharray='2 2'" : "";
        var frame = disabled ? t.Rule : on ? t.Ink : t.Rule;
        var fill = on ? (disabled ? t.Hairline : t.Ink) : t.Paper;
        var tick = on ? $"<path d='M4 8.25 6.75 11 12 5' {string.Format(Stroke, Hex(disabled ? t.InkMuted : t.Paper))}/>" : "";
        return FromSvg($"<svg xmlns='http://www.w3.org/2000/svg' width='16' height='16' viewBox='0 0 16 16'>" +
                       $"<rect x='0.5' y='0.5' width='15' height='15' rx='{t.Radius}' fill='{Hex(fill)}' stroke='{Hex(frame)}'{dash}/>{tick}</svg>");
    }

    /// <summary>Switch: a ruled slot with a square carriage; on fills the slot with the action colour.</summary>
    public static Texture2D Switch(bool on, bool disabled, Tokens t)
    {
        var dash = disabled ? " stroke-dasharray='2 2'" : "";
        var slot = on ? (disabled ? t.Hairline : t.Action) : t.Paper;
        var frame = disabled ? t.Rule : on ? t.Action : t.Rule;
        var carriage = on ? (disabled ? t.InkMuted : t.OnAction) : (disabled ? t.Hairline : t.Ink);
        var x = on ? 17 : 3;
        return FromSvg($"<svg xmlns='http://www.w3.org/2000/svg' width='32' height='16' viewBox='0 0 32 16'>" +
                       $"<rect x='0.5' y='0.5' width='31' height='15' rx='{t.Radius}' fill='{Hex(slot)}' stroke='{Hex(frame)}'{dash}/>" +
                       $"<rect x='{x}' y='3' width='12' height='10' rx='1' fill='{Hex(carriage)}'/></svg>");
    }

    /// <summary>Slider carriage: a vertical index mark, like a reading on a scale.</summary>
    public static Texture2D Tick(Color color) =>
        FromSvg($"<svg xmlns='http://www.w3.org/2000/svg' width='8' height='20' viewBox='0 0 8 20'><rect x='1' y='0' width='6' height='20' rx='1' fill='{Hex(color)}'/></svg>");

    /// <summary>Scale graduation: a short hairline.</summary>
    public static Texture2D Graduation(Color color) =>
        FromSvg($"<svg xmlns='http://www.w3.org/2000/svg' width='2' height='8' viewBox='0 0 2 8'><rect x='0.5' y='0' width='1' height='8' fill='{Hex(color)}'/></svg>");

    static Texture2D FromSvg(string svg) => DpiTexture.CreateFromString(svg);

    static string Hex(Color c) => "#" + c.ToHtml(false);
}
