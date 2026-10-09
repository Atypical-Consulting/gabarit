using System;
using System.Linq;
using Godot;
using Godot.Collections;

namespace Gabarit;

/// <summary>
/// Edits the exported properties of any object, one ruled row per property:
/// key on the left, an editor chosen from the property's type and hint on the right.
/// Supports bool, int, float, enum, string and Color; other types show their value read-only.
/// </summary>
[GlobalClass]
public partial class PropertyInspector : VBoxContainer
{
    [Signal] public delegate void PropertyEditedEventHandler(StringName property);

    GodotObject? _target;

    public PropertyInspector() => AddThemeConstantOverride("separation", 0);

    // Rows bake theme colours into their overrides, so a theme switch rebuilds them.
    public override void _Notification(int what)
    {
        if (what == NotificationThemeChanged && _target is not null) Callable.From(() => Inspect(_target)).CallDeferred();
    }

    /// <summary>Rebuild the rows for <paramref name="target"/>'s script-exported properties.</summary>
    public void Inspect(GodotObject? target)
    {
        _target = target;
        foreach (var child in GetChildren()) child.QueueFree();
        if (target is null) return;

        string? group = null; // a group header is only drawn once a property of it is shown
        foreach (var p in target.GetPropertyList())
        {
            var usage = (PropertyUsageFlags)(int)p["usage"];
            var name = (string)p["name"];
            if (usage.HasFlag(PropertyUsageFlags.Group)) { group = name; continue; }
            if (!usage.HasFlag(PropertyUsageFlags.ScriptVariable) || !usage.HasFlag(PropertyUsageFlags.Editor)) continue;
            if (group is not null) { AddChild(GroupRow(group)); group = null; }
            AddChild(Row(name, MakeEditor(target, p)));
        }
    }

    Control MakeEditor(GodotObject target, Dictionary p)
    {
        var name = (string)p["name"];
        var type = (Variant.Type)(int)p["type"];
        var hint = (PropertyHint)(int)p["hint"];
        var hintString = (string)p["hint_string"];
        var value = target.Get(name);

        void Commit(Variant v)
        {
            target.Set(name, v);
            EmitSignal(SignalName.PropertyEdited, name);
        }

        switch (type)
        {
            case Variant.Type.Bool:
                var check = new CheckBox { ButtonPressed = value.AsBool(), Text = value.AsBool() ? "On" : "Off" };
                check.Toggled += on => { check.Text = on ? "On" : "Off"; Commit(on); };
                return check;

            case Variant.Type.Int when hint == PropertyHint.Enum:
                var select = new OptionButton { SizeFlagsHorizontal = SizeFlags.ExpandFill };
                var options = hintString.Split(',');
                for (int i = 0; i < options.Length; i++)
                {
                    var parts = options[i].Split(':');
                    select.AddItem(parts[0], parts.Length > 1 ? int.Parse(parts[1]) : i);
                }
                select.Select(select.GetItemIndex(value.AsInt32()));
                select.ItemSelected += idx => Commit(select.GetItemId((int)idx));
                return select;

            case Variant.Type.Int or Variant.Type.Float:
                var spin = new SpinBox
                {
                    SizeFlagsHorizontal = SizeFlags.ExpandFill,
                    AllowGreater = true, AllowLesser = true,
                    MinValue = -1e9, MaxValue = 1e9,
                    Step = type == Variant.Type.Int ? 1 : 0.01,
                };
                if (hint == PropertyHint.Range)
                {
                    var r = hintString.Split(',');
                    spin.MinValue = double.Parse(r[0], System.Globalization.CultureInfo.InvariantCulture);
                    spin.MaxValue = double.Parse(r[1], System.Globalization.CultureInfo.InvariantCulture);
                    if (r.Length > 2 && double.TryParse(r[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var step))
                    {
                        // SpinBox shows only as many decimals as the step's first digit (0.25 displays "1.2"):
                        // keep the hinted step on the arrows and a finer step for display.
                        spin.CustomArrowStep = step;
                        spin.Step = type == Variant.Type.Float ? Math.Min(step, 0.01) : step;
                    }
                    spin.AllowGreater = spin.AllowLesser = false;
                }
                spin.Value = value.AsDouble();
                spin.ValueChanged += v => Commit(type == Variant.Type.Int ? (int)v : v);
                return spin;

            case Variant.Type.String or Variant.Type.StringName:
                var edit = new LineEdit { Text = value.AsString(), SizeFlagsHorizontal = SizeFlags.ExpandFill };
                edit.TextSubmitted += v => Commit(v);
                edit.FocusExited += () => { if (edit.Text != target.Get(name).AsString()) Commit(edit.Text); };
                return edit;

            case Variant.Type.Color:
                var color = new ColorPickerButton { Color = value.AsColor(), CustomMinimumSize = new Vector2(72, 26), EditAlpha = hint != PropertyHint.ColorNoAlpha };
                color.ColorChanged += c => Commit(c);
                return color;

            default:
                return new Label { Text = value.ToString(), ThemeTypeVariation = "CaptionLabel" };
        }
    }

    PanelContainer Row(string name, Control editor)
    {
        var key = new Label
        {
            Text = Humanize(name),
            CustomMinimumSize = new Vector2(GetThemeConstant("key_width", "PropertyInspector"), 0),
            VerticalAlignment = VerticalAlignment.Center,
            TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis,
            MouseFilter = MouseFilterEnum.Stop,
        };
        key.AddThemeColorOverride("font_color", GetThemeColor("key_color", "PropertyInspector"));
        key.GuiInput += e =>
        {
            if (e is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left } && editor.FocusMode != FocusModeEnum.None)
                editor.GrabFocus();
        };
        editor.TooltipText = name;
        editor.SizeFlagsVertical = SizeFlags.ShrinkCenter;

        var line = new HBoxContainer { CustomMinimumSize = new Vector2(0, GetThemeConstant("row_height", "PropertyInspector")) };
        line.AddThemeConstantOverride("separation", 12);
        line.AddChild(key);
        line.AddChild(editor);
        return Ruled(line, Colors.Transparent);
    }

    static readonly string[] Acronyms = ["UI", "ID", "URL", "URI", "API", "FPS", "HDR", "RGB", "UV", "XR", "DPI", "CPU", "GPU", "IO"];

    /// <summary>"UiScale" → "UI Scale": Godot's Capitalize, then known acronyms restored.</summary>
    public static string Humanize(string name) =>
        string.Join(' ', name.Capitalize().Split(' ').Select(w => Acronyms.FirstOrDefault(a => a.Equals(w, StringComparison.OrdinalIgnoreCase)) ?? w));

    PanelContainer GroupRow(string name)
    {
        var label = new Label { Text = name, ThemeTypeVariation = "StrongLabel", VerticalAlignment = VerticalAlignment.Center, CustomMinimumSize = new Vector2(0, 28) };
        return Ruled(label, GetThemeColor("group_color", "PropertyInspector"));
    }

    PanelContainer Ruled(Control content, Color fill)
    {
        var box = new StyleBoxFlat
        {
            BgColor = fill,
            BorderColor = GetThemeColor("rule_color", "PropertyInspector"),
            BorderWidthBottom = 1,
            ContentMarginLeft = 8, ContentMarginRight = 8, ContentMarginTop = 2, ContentMarginBottom = 2,
        };
        var panel = new PanelContainer();
        panel.AddThemeStyleboxOverride("panel", box);
        panel.AddChild(content);
        return panel;
    }
}
