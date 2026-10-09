using Godot;

namespace Gabarit;

/// <summary>One icon from <see cref="Icons"/>, drawn in the theme's ink (Label font_color) unless <see cref="Tint"/> is set.</summary>
[GlobalClass]
public partial class Icon : Control
{
    string _name = "check";
    [Export] public string IconName { get => _name; set { _name = value; QueueRedraw(); } }

    Color? _tint;
    public Color? Tint { get => _tint; set { _tint = value; QueueRedraw(); } }

    public Icon()
    {
        CustomMinimumSize = new Vector2(16, 16);
        MouseFilter = MouseFilterEnum.Ignore;
    }

    public override void _Notification(int what)
    {
        if (what == NotificationThemeChanged) QueueRedraw();
    }

    public override void _Draw()
    {
        var tex = Icons.Get(_name, _tint ?? GetThemeColor("font_color", "Label"));
        var s = new Vector2(16, 16);
        DrawTextureRect(tex, new Rect2(((Size - s) / 2).Round(), s), false);
    }
}
