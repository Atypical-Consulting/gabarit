using Godot;

namespace Gabarit;

/// <summary>
/// Turns <see cref="Tokens"/> into a Godot <see cref="Theme"/> that styles the native controls
/// (Button, LineEdit, CheckBox, CheckButton, OptionButton, HSlider, Tree, ItemList…) and the
/// library's own (DataGrid, PropertyInspector, Field).
/// Assign the result to the root Control of your app: <c>root.Theme = ThemeBuilder.Build(Tokens.CreatePaper());</c>
/// </summary>
public static class ThemeBuilder
{
    public static Theme Build(Tokens t)
    {
        var th = new Theme
        {
            DefaultFont = t.Regular,
            DefaultFontSize = t.FontSize,
        };
        var u = t.Unit;
        var focus = Focus(t);

        // Label and its variations
        th.SetColor("font_color", "Label", t.Ink);
        Variation(th, "TitleLabel", "Label");
        th.SetFont("font", "TitleLabel", t.Bold);
        th.SetFontSize("font_size", "TitleLabel", t.TitleSize);
        Variation(th, "HeadingLabel", "Label");
        th.SetFont("font", "HeadingLabel", t.Bold);
        th.SetFontSize("font_size", "HeadingLabel", t.FontSize + 4);
        Variation(th, "StrongLabel", "Label");
        th.SetFont("font", "StrongLabel", t.Bold);
        Variation(th, "CaptionLabel", "Label");
        th.SetFontSize("font_size", "CaptionLabel", t.SmallSize);
        th.SetColor("font_color", "CaptionLabel", t.InkMuted);
        Variation(th, "FieldLabel", "Label");
        th.SetFont("font", "FieldLabel", t.Bold);
        th.SetFontSize("font_size", "FieldLabel", t.SmallSize);
        Variation(th, "ErrorLabel", "Label");
        th.SetFontSize("font_size", "ErrorLabel", t.SmallSize);
        th.SetColor("font_color", "ErrorLabel", t.Danger);

        // Button: pressed is filled ink, disabled is a dashed rule.
        ButtonLike(th, "Button", t, focus,
            normal: Box(t.Paper, t.Rule, t, 3 * u, u),
            hover: Box(t.Band, t.Ink, t, 3 * u, u),
            pressed: Box(t.Ink, t.Ink, t, 3 * u, u),
            disabled: Dashed(t.Paper, t.Rule, 3 * u, u),
            text: t.Ink, pressedText: t.Paper);
        th.SetConstant("h_separation", "Button", 2 * u);

        Variation(th, "PrimaryButton", "Button");
        ButtonLike(th, "PrimaryButton", t, focus,
            normal: Box(t.Action, t.Action, t, 3 * u, u),
            hover: Box(t.Action.Darkened(0.15f), t.Action.Darkened(0.15f), t, 3 * u, u),
            pressed: Box(t.Ink, t.Ink, t, 3 * u, u),
            disabled: Dashed(t.Paper, t.Rule, 3 * u, u),
            text: t.OnAction, pressedText: t.Paper);
        th.SetFont("font", "PrimaryButton", t.Bold);

        // A quiet button for toolbars and table headers: no frame until hovered.
        Variation(th, "FlatButton", "Button");
        ButtonLike(th, "FlatButton", t, focus,
            normal: Box(Colors.Transparent, Colors.Transparent, t, 2 * u, u),
            hover: Box(t.Band, t.Band, t, 2 * u, u),
            pressed: Box(t.Ink, t.Ink, t, 2 * u, u),
            disabled: Box(Colors.Transparent, Colors.Transparent, t, 2 * u, u),
            text: t.Ink, pressedText: t.Paper);

        // OptionButton (Select) keeps ink text while its popup is open.
        ButtonLike(th, "OptionButton", t, focus,
            normal: Box(t.Paper, t.Rule, t, 3 * u, u, right: 3 * u),
            hover: Box(t.Paper, t.Ink, t, 3 * u, u, right: 3 * u),
            pressed: Box(t.Band, t.Ink, t, 3 * u, u, right: 3 * u),
            disabled: Dashed(t.Paper, t.Rule, 3 * u, u),
            text: t.Ink, pressedText: t.Ink);
        th.SetIcon("arrow", "OptionButton", Icons.Get("chevron-down", t.Ink));
        th.SetConstant("arrow_margin", "OptionButton", 2 * u);
        th.SetConstant("modulate_arrow", "OptionButton", 1);

        th.SetStylebox("panel", "PopupMenu", Popup(t));
        th.SetStylebox("hover", "PopupMenu", Box(t.ActionTint, t.ActionTint, t, 2 * u, u));
        th.SetStylebox("separator", "PopupMenu", Line(t.Hairline, false));
        th.SetColor("font_color", "PopupMenu", t.Ink);
        th.SetColor("font_hover_color", "PopupMenu", t.Ink);
        th.SetColor("font_disabled_color", "PopupMenu", t.InkMuted);
        th.SetColor("font_separator_color", "PopupMenu", t.InkMuted);
        th.SetConstant("v_separation", "PopupMenu", 2 * u);
        th.SetConstant("item_start_padding", "PopupMenu", 3 * u);
        th.SetConstant("item_end_padding", "PopupMenu", 3 * u);
        th.SetIcon("radio_checked", "PopupMenu", Icons.Get("check", t.Ink));
        th.SetIcon("checked", "PopupMenu", Icons.Get("check", t.Ink));
        th.SetIcon("radio_unchecked", "PopupMenu", Empty16());
        th.SetIcon("unchecked", "PopupMenu", Empty16());
        th.SetIcon("submenu", "PopupMenu", Icons.Get("chevron-right", t.Ink));

        // CheckBox and CheckButton (Switch): the mark carries the state, the label never inverts.
        foreach (var type in new[] { "CheckBox", "CheckButton" })
        {
            var sw = type == "CheckButton";
            var none = Box(Colors.Transparent, Colors.Transparent, t, 0, u, left: 0);
            th.SetStylebox("normal", type, none);
            th.SetStylebox("pressed", type, none);
            th.SetStylebox("hover", type, none);
            th.SetStylebox("hover_pressed", type, none);
            th.SetStylebox("disabled", type, none);
            th.SetStylebox("focus", type, focus);
            foreach (var c in new[] { "font_color", "font_pressed_color", "font_hover_color", "font_hover_pressed_color", "font_focus_color" })
                th.SetColor(c, type, t.Ink);
            th.SetColor("font_disabled_color", type, t.InkMuted);
            th.SetConstant("h_separation", type, 2 * u);
            Texture2D Mark(bool on, bool off) => sw ? Icons.Switch(on, off, t) : Icons.Box(on, off, t);
            th.SetIcon("checked", type, Mark(true, false));
            th.SetIcon("unchecked", type, Mark(false, false));
            th.SetIcon("checked_disabled", type, Mark(true, true));
            th.SetIcon("unchecked_disabled", type, Mark(false, true));
            if (sw)
            {
                th.SetIcon("checked_mirrored", type, Mark(true, false));
                th.SetIcon("unchecked_mirrored", type, Mark(false, false));
                th.SetIcon("checked_disabled_mirrored", type, Mark(true, true));
                th.SetIcon("unchecked_disabled_mirrored", type, Mark(false, true));
            }
        }

        // LineEdit (TextField)
        foreach (var type in new[] { "LineEdit", "TextEdit" })
        {
            th.SetStylebox("normal", type, Box(t.Paper, t.Rule, t, 2 * u, u));
            th.SetStylebox("focus", type, Box(t.Paper, t.Ink, t, 2 * u, u, border: 2));
            th.SetStylebox("read_only", type, Dashed(t.Ground, t.Rule, 2 * u, u));
            th.SetColor("font_color", type, t.Ink);
            th.SetColor("font_readonly_color", type, t.InkMuted);
            th.SetColor("font_uneditable_color", type, t.InkMuted);
            th.SetColor("font_placeholder_color", type, t.InkMuted);
            th.SetColor("font_selected_color", type, t.Ink);
            th.SetColor("selection_color", type, t.ActionTint);
            th.SetColor("caret_color", type, t.Ink);
            th.SetConstant("caret_width", type, 2);
            th.SetIcon("clear", type, Icons.Get("close", t.InkMuted));
        }
        Variation(th, "ErrorLineEdit", "LineEdit");
        th.SetStylebox("normal", "ErrorLineEdit", Box(t.Paper, t.Danger, t, 2 * u, u));
        th.SetStylebox("focus", "ErrorLineEdit", Box(t.Paper, t.Danger, t, 2 * u, u, border: 2));

        th.SetIcon("up", "SpinBox", Icons.Get("chevron-up", t.Ink));
        th.SetIcon("down", "SpinBox", Icons.Get("chevron-down", t.Ink));
        th.SetIcon("up_disabled", "SpinBox", Icons.Get("chevron-up", t.Rule));
        th.SetIcon("down_disabled", "SpinBox", Icons.Get("chevron-down", t.Rule));
        th.SetIcon("up_hover", "SpinBox", Icons.Get("chevron-up", t.Action));
        th.SetIcon("down_hover", "SpinBox", Icons.Get("chevron-down", t.Action));
        th.SetIcon("up_pressed", "SpinBox", Icons.Get("chevron-up", t.Ink));
        th.SetIcon("down_pressed", "SpinBox", Icons.Get("chevron-down", t.Ink));

        // Slider: a ruled scale, ink up to the reading, a tick for the carriage.
        foreach (var type in new[] { "HSlider", "VSlider" })
        {
            var track = new StyleBoxFlat { BgColor = t.Hairline, ContentMarginTop = 1, ContentMarginBottom = 1, ContentMarginLeft = 1, ContentMarginRight = 1 };
            th.SetStylebox("slider", type, track);
            th.SetStylebox("grabber_area", type, Flat(t.Ink, 1));
            th.SetStylebox("grabber_area_highlight", type, Flat(t.Action, 1));
            th.SetIcon("grabber", type, Icons.Tick(t.Ink));
            th.SetIcon("grabber_highlight", type, Icons.Tick(t.Action));
            th.SetIcon("grabber_disabled", type, Icons.Tick(t.Rule));
            th.SetIcon("tick", type, Icons.Graduation(t.Rule));
            th.SetStylebox("focus", type, focus);
            th.SetConstant("center_grabber", type, 0);
            th.SetConstant("grabber_offset", type, 0);
        }

        // Scrollbars: thin graphite carriages, no trough.
        foreach (var type in new[] { "VScrollBar", "HScrollBar" })
        {
            var v = type == "VScrollBar";
            var trough = new StyleBoxEmpty();
            trough.SetContentMargin(v ? Side.Left : Side.Top, 3);
            trough.SetContentMargin(v ? Side.Right : Side.Bottom, 3);
            th.SetStylebox("scroll", type, trough);
            th.SetStylebox("scroll_focus", type, trough);
            th.SetStylebox("grabber", type, Grabber(t.Rule, v));
            th.SetStylebox("grabber_highlight", type, Grabber(t.InkMuted, v));
            th.SetStylebox("grabber_pressed", type, Grabber(t.Ink, v));
            foreach (var i in new[] { "increment", "increment_highlight", "increment_pressed", "decrement", "decrement_highlight", "decrement_pressed" })
                th.SetIcon(i, type, new ImageTexture());
        }

        // Tree (TreeView) and ItemList: ruled frame, 90° guides, tinted selection.
        foreach (var type in new[] { "Tree", "ItemList" })
        {
            th.SetStylebox("panel", type, Box(t.Paper, t.Rule, t, 1, 1));
            th.SetStylebox("focus", type, focus);
            th.SetStylebox("selected", type, Flat(t.ActionTint, 0));
            th.SetStylebox("selected_focus", type, Flat(t.ActionTint, 0));
            th.SetStylebox("cursor", type, Outline(t.Ink, 1));
            th.SetStylebox("cursor_unfocused", type, new StyleBoxEmpty());
            th.SetStylebox("hovered", type, Flat(t.Band, 0));
            th.SetStylebox("hovered_selected", type, Flat(t.ActionTint, 0));
            th.SetStylebox("hovered_selected_focus", type, Flat(t.ActionTint, 0));
            th.SetColor("font_color", type, t.Ink);
            th.SetColor("font_selected_color", type, t.Ink);
            th.SetColor("font_hovered_color", type, t.Ink);
            th.SetColor("font_hovered_selected_color", type, t.Ink);
            th.SetColor("guide_color", type, t.Hairline);
            th.SetConstant("v_separation", type, u);
            th.SetConstant("h_separation", type, 2 * u);
        }
        th.SetColor("relationship_line_color", "Tree", t.Rule);
        th.SetColor("parent_hl_line_color", "Tree", t.Ink);
        th.SetColor("children_hl_line_color", "Tree", t.Rule);
        th.SetConstant("draw_relationship_lines", "Tree", 1);
        th.SetConstant("draw_guides", "Tree", 1);
        th.SetConstant("relationship_line_width", "Tree", 1);
        th.SetConstant("parent_hl_line_width", "Tree", 1);
        th.SetConstant("children_hl_line_width", "Tree", 1);
        th.SetConstant("item_margin", "Tree", 5 * u);
        th.SetConstant("inner_item_margin_left", "Tree", u);
        th.SetConstant("inner_item_margin_right", "Tree", u);
        th.SetStylebox("title_button_normal", "Tree", Box(t.Band, t.Hairline, t, 2 * u, u));
        th.SetStylebox("title_button_hover", "Tree", Box(t.Hairline, t.Hairline, t, 2 * u, u));
        th.SetStylebox("title_button_pressed", "Tree", Box(t.Hairline, t.Hairline, t, 2 * u, u));
        th.SetColor("title_button_color", "Tree", t.Ink);
        th.SetFont("title_button_font", "Tree", t.Bold);
        th.SetIcon("arrow", "Tree", Icons.Get("chevron-down", t.Ink));
        th.SetIcon("arrow_collapsed", "Tree", Icons.Get("chevron-right", t.Ink));
        th.SetIcon("arrow_collapsed_mirrored", "Tree", Icons.Get("chevron-right", t.Ink));
        th.SetIcon("checked", "Tree", Icons.Box(true, false, t));
        th.SetIcon("unchecked", "Tree", Icons.Box(false, false, t));
        th.SetIcon("checked_disabled", "Tree", Icons.Box(true, true, t));
        th.SetIcon("unchecked_disabled", "Tree", Icons.Box(false, true, t));

        // Panels and separators
        th.SetStylebox("panel", "PanelContainer", Flat(t.Paper, 0));
        Variation(th, "SheetPanel", "PanelContainer");
        th.SetStylebox("panel", "SheetPanel", Box(t.Paper, t.Rule, t, 0, 0));
        Variation(th, "GroundPanel", "PanelContainer");
        th.SetStylebox("panel", "GroundPanel", Flat(t.Ground, 0));
        Variation(th, "BandPanel", "PanelContainer");
        th.SetStylebox("panel", "BandPanel", Flat(t.Band, 0));
        th.SetStylebox("panel", "Panel", Flat(t.Paper, 0));
        th.SetStylebox("separator", "HSeparator", Line(t.Hairline, false));
        th.SetStylebox("separator", "VSeparator", Line(t.Hairline, true));
        th.SetConstant("separation", "HSeparator", 1);
        th.SetConstant("separation", "VSeparator", 1);

        // Tooltip: the one inverted surface.
        th.SetStylebox("panel", "TooltipPanel", Box(t.Ink, t.Ink, t, 2 * u, u + 2));
        th.SetColor("font_color", "TooltipLabel", t.Paper);
        th.SetFontSize("font_size", "TooltipLabel", t.SmallSize);

        // Library controls read these.
        const string g = "DataGrid";
        th.SetStylebox("panel", g, Box(t.Paper, t.Rule, t, 0, 0));
        th.SetStylebox("focus", g, focus);
        th.SetColor("header_color", g, t.Band);
        th.SetColor("rule_color", g, t.Hairline);
        th.SetColor("frame_color", g, t.Rule);
        th.SetColor("font_color", g, t.Ink);
        th.SetColor("font_muted_color", g, t.InkMuted);
        th.SetColor("selected_color", g, t.ActionTint);
        th.SetColor("hover_color", g, t.Ground);
        th.SetColor("cursor_color", g, t.Ink);
        th.SetColor("accent_color", g, t.Action);
        th.SetFont("font", g, t.Regular);
        th.SetFont("header_font", g, t.Bold);
        th.SetFontSize("font_size", g, t.FontSize);
        th.SetFontSize("small_font_size", g, t.SmallSize);
        th.SetConstant("row_height", g, t.RowHeight);
        th.SetConstant("header_height", g, t.RowHeight + u);
        th.SetConstant("footer_height", g, t.RowHeight);
        th.SetConstant("cell_padding", g, 2 * u);
        th.SetIcon("sort_asc", g, Icons.Get("sort-asc", t.Ink));
        th.SetIcon("sort_desc", g, Icons.Get("sort-desc", t.Ink));

        th.SetColor("key_color", "PropertyInspector", t.InkMuted);
        th.SetColor("rule_color", "PropertyInspector", t.Hairline);
        th.SetColor("group_color", "PropertyInspector", t.Band);
        th.SetConstant("row_height", "PropertyInspector", t.ControlHeight + u);
        th.SetConstant("key_width", "PropertyInspector", 40 * u);

        foreach (var type in new[] { "Button", "LineEdit", "OptionButton", "SpinBox" })
            th.SetConstant("minimum_height", type, t.ControlHeight);
        return th;
    }

    static void Variation(Theme th, string name, string baseType) => th.SetTypeVariation(name, baseType);

    static void ButtonLike(Theme th, string type, Tokens t, StyleBox focus, StyleBox normal, StyleBox hover,
        StyleBox pressed, StyleBox disabled, Color text, Color pressedText)
    {
        th.SetStylebox("normal", type, normal);
        th.SetStylebox("hover", type, hover);
        th.SetStylebox("pressed", type, pressed);
        th.SetStylebox("hover_pressed", type, pressed);
        th.SetStylebox("disabled", type, disabled);
        th.SetStylebox("focus", type, focus);
        th.SetColor("font_color", type, text);
        th.SetColor("font_hover_color", type, text == t.OnAction ? text : t.Ink);
        th.SetColor("font_focus_color", type, text);
        th.SetColor("font_pressed_color", type, pressedText);
        th.SetColor("font_hover_pressed_color", type, pressedText);
        th.SetColor("font_disabled_color", type, t.InkMuted);
        th.SetColor("icon_normal_color", type, text);
        th.SetColor("icon_hover_color", type, text == t.OnAction ? text : t.Ink);
        th.SetColor("icon_pressed_color", type, pressedText);
        th.SetColor("icon_hover_pressed_color", type, pressedText);
        th.SetColor("icon_focus_color", type, text);
        th.SetColor("icon_disabled_color", type, t.Rule);
    }

    /// <summary>Focus is a 2px ink frame set 2px outside the control. It is the only thing that draws there.</summary>
    static StyleBoxFlat Focus(Tokens t) => new()
    {
        DrawCenter = false,
        BorderColor = t.Ink,
        BorderWidthLeft = 2, BorderWidthTop = 2, BorderWidthRight = 2, BorderWidthBottom = 2,
        ExpandMarginLeft = 2, ExpandMarginTop = 2, ExpandMarginRight = 2, ExpandMarginBottom = 2,
        CornerRadiusTopLeft = t.Radius, CornerRadiusTopRight = t.Radius,
        CornerRadiusBottomLeft = t.Radius, CornerRadiusBottomRight = t.Radius,
    };

    internal static StyleBoxFlat Box(Color fill, Color stroke, Tokens t, float h, float v, float? left = null, float? right = null, int border = 1)
    {
        var b = new StyleBoxFlat
        {
            BgColor = fill,
            BorderColor = stroke,
            CornerRadiusTopLeft = t.Radius, CornerRadiusTopRight = t.Radius,
            CornerRadiusBottomLeft = t.Radius, CornerRadiusBottomRight = t.Radius,
            ContentMarginLeft = left ?? h, ContentMarginRight = right ?? h,
            ContentMarginTop = v, ContentMarginBottom = v,
            AntiAliasing = false,
        };
        b.SetBorderWidthAll(border);
        return b;
    }

    static StyleBoxFlat Flat(Color fill, float margin) => new()
    {
        BgColor = fill,
        ContentMarginLeft = margin, ContentMarginRight = margin, ContentMarginTop = margin, ContentMarginBottom = margin,
    };

    static StyleBoxFlat Outline(Color color, int width)
    {
        var b = new StyleBoxFlat { DrawCenter = false, BorderColor = color, AntiAliasing = false };
        b.SetBorderWidthAll(width);
        return b;
    }

    static StyleBoxFlat Grabber(Color color, bool vertical)
    {
        var b = new StyleBoxFlat { BgColor = color };
        b.SetCornerRadiusAll(1);
        b.SetContentMargin(vertical ? Side.Left : Side.Top, 3);
        b.SetContentMargin(vertical ? Side.Right : Side.Bottom, 3);
        b.SetContentMargin(vertical ? Side.Top : Side.Left, 12);
        b.SetContentMargin(vertical ? Side.Bottom : Side.Right, 12);
        return b;
    }

    static StyleBoxFlat Popup(Tokens t)
    {
        var b = Box(t.Paper, t.Rule, t, t.Unit, t.Unit);
        b.ShadowColor = new Color(0, 0, 0, 0.18f);
        b.ShadowSize = 10;
        b.ShadowOffset = new Vector2(0, 3);
        return b;
    }

    static StyleBoxLine Line(Color color, bool vertical) => new() { Color = color, Thickness = 1, Vertical = vertical };

    static StyleBox Dashed(Color fill, Color border, float h, float v) => new DashedBox
    {
        Fill = fill, Border = border,
        ContentMarginLeft = h, ContentMarginRight = h, ContentMarginTop = v, ContentMarginBottom = v,
    };

    static Texture2D Empty16()
    {
        var img = Image.CreateEmpty(16, 16, false, Image.Format.Rgba8);
        return ImageTexture.CreateFromImage(img);
    }
}

/// <summary>A rectangle with a dashed 1px rule: the library's mark for "not available".</summary>
[GlobalClass]
public partial class DashedBox : StyleBox
{
    [Export] public Color Fill { get; set; }
    [Export] public Color Border { get; set; }
    [Export] public int Dash { get; set; } = 3;
    [Export] public int Gap { get; set; } = 3;

    public override void _Draw(Rid canvasItem, Rect2 rect)
    {
        rect = new Rect2(rect.Position.Round(), rect.Size.Round());
        if (Fill.A > 0) RenderingServer.CanvasItemAddRect(canvasItem, rect, Fill);
        var step = Dash + Gap;
        for (float x = rect.Position.X; x < rect.End.X; x += step)
        {
            var w = Mathf.Min(Dash, rect.End.X - x);
            RenderingServer.CanvasItemAddRect(canvasItem, new Rect2(x, rect.Position.Y, w, 1), Border);
            RenderingServer.CanvasItemAddRect(canvasItem, new Rect2(x, rect.End.Y - 1, w, 1), Border);
        }
        for (float y = rect.Position.Y; y < rect.End.Y; y += step)
        {
            var hgt = Mathf.Min(Dash, rect.End.Y - y);
            RenderingServer.CanvasItemAddRect(canvasItem, new Rect2(rect.Position.X, y, 1, hgt), Border);
            RenderingServer.CanvasItemAddRect(canvasItem, new Rect2(rect.End.X - 1, y, 1, hgt), Border);
        }
    }
}
