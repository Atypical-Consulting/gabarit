using Godot;

namespace Gabarit;

/// <summary>
/// A labelled text input: label above, the <see cref="LineEdit"/>, then help or error text below.
/// The error line replaces the help line in the same slot, so the field never changes height.
/// </summary>
[GlobalClass]
public partial class Field : VBoxContainer
{
    readonly Label _label = new() { ThemeTypeVariation = "FieldLabel" };
    readonly Label _note = new() { ThemeTypeVariation = "CaptionLabel", AutowrapMode = TextServer.AutowrapMode.WordSmart };

    public LineEdit Input { get; } = new();

    public Field()
    {
        AddThemeConstantOverride("separation", 4);
        AddChild(_label, false, InternalMode.Front);
        AddChild(Input, false, InternalMode.Front);
        AddChild(_note, false, InternalMode.Front);
        // Clicking the label focuses the input, like an HTML <label for>.
        _label.MouseFilter = MouseFilterEnum.Stop;
        _label.GuiInput += e =>
        {
            if (e is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left }) Input.GrabFocus();
        };
        Input.TextChanged += _ => { if (Error != "") Error = ""; };
    }

    [Export] public string Label { get => _label.Text; set => _label.Text = value; }
    [Export] public string Text { get => Input.Text; set => Input.Text = value; }
    [Export] public string Placeholder { get => Input.PlaceholderText; set => Input.PlaceholderText = value; }

    string _help = "";
    [Export] public string Help { get => _help; set { _help = value; Refresh(); } }

    string _error = "";
    /// <summary>Set to show an error; editing the text clears it.</summary>
    [Export] public string Error { get => _error; set { _error = value; Refresh(); } }

    [Export] public bool Editable
    {
        get => Input.Editable;
        set { Input.Editable = value; Input.FocusMode = value ? FocusModeEnum.All : FocusModeEnum.None; }
    }

    void Refresh()
    {
        var failing = _error != "";
        _note.Text = failing ? _error : _help;
        _note.ThemeTypeVariation = failing ? "ErrorLabel" : "CaptionLabel";
        Input.ThemeTypeVariation = failing ? "ErrorLineEdit" : "";
        Input.TooltipText = failing ? _error : "";
    }
}
