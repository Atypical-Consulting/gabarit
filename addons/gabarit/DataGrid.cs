using System;
using System.Collections.Generic;
using System.Globalization;
using Godot;

namespace Gabarit;

/// <summary>One column of a <see cref="DataGrid"/>.</summary>
public sealed class GridColumn
{
    public string Title { get; set; } = "";
    public float Width { get; set; } = 120;
    public float MinWidth { get; set; } = 48;
    /// <summary>Right-aligned, tabular figures.</summary>
    public bool Numeric { get; set; }
    /// <summary>Takes the remaining width when the columns are narrower than the grid.</summary>
    public bool Expand { get; set; }
    public bool Sortable { get; set; } = true;
}

/// <summary>
/// A virtualized, fully ruled data table. Only visible rows are drawn, so row count is bounded
/// by memory of your data source, not by nodes. Supply <see cref="RowCount"/> and <see cref="CellText"/>;
/// sorting is yours to perform on <see cref="SortRequested"/>.
/// </summary>
[GlobalClass]
public partial class DataGrid : Control
{
    [Signal] public delegate void SelectionChangedEventHandler(int row);
    [Signal] public delegate void RowActivatedEventHandler(int row);
    [Signal] public delegate void SortRequestedEventHandler(int column, bool ascending);
    [Signal] public delegate void VisibleRangeChangedEventHandler(int first, int last, int total);

    public List<GridColumn> Columns { get; } = new();
    public Func<int, int, string>? CellText { get; set; }

    [Export] public bool ShowHeader { get; set; } = true;
    /// <summary>The footer carries the visible-range guide words.</summary>
    [Export] public bool ShowFooter { get; set; } = true;
    /// <summary>{0} first visible row, {1} last, {2} total.</summary>
    [Export] public string RangeFormat { get; set; } = "Rows {0}–{1} of {2}";
    [Export] public string EmptyText { get; set; } = "No rows";

    int _rowCount;
    [Export] public int RowCount
    {
        get => _rowCount;
        set { _rowCount = Math.Max(0, value); if (SelectedRow >= _rowCount) SelectedRow = _rowCount - 1; Relayout(); }
    }

    int _selected = -1;
    public int SelectedRow
    {
        get => _selected;
        set
        {
            var v = Math.Clamp(value, -1, _rowCount - 1);
            if (v == _selected) return;
            _selected = v;
            EmitSignal(SignalName.SelectionChanged, v);
            QueueRedraw();
        }
    }

    public int SortColumn { get; private set; } = -1;
    public bool SortAscending { get; private set; } = true;

    readonly VScrollBar _v = new();
    readonly HScrollBar _h = new();
    int _hoverRow = -1;
    int _resizing = -1;
    float _resizeStartX, _resizeStartW;
    (int, int, int) _lastRange = (-1, -1, -1);

    public DataGrid()
    {
        FocusMode = FocusModeEnum.All;
        ClipContents = true;
        CustomMinimumSize = new Vector2(160, 120);
        _v.ValueChanged += _ => OnScrolled();
        _h.ValueChanged += _ => QueueRedraw();
        _v.FocusMode = _h.FocusMode = FocusModeEnum.None;
        AddChild(_v, false, InternalMode.Front);
        AddChild(_h, false, InternalMode.Front);
    }

    /// <summary>Replace the columns and data source in one call.</summary>
    public void SetData(IEnumerable<GridColumn> columns, int rowCount, Func<int, int, string> cellText)
    {
        Columns.Clear();
        Columns.AddRange(columns);
        CellText = cellText;
        RowCount = rowCount;
    }

    /// <summary>Mark a column as sorted without emitting <see cref="SortRequested"/>.</summary>
    public void SetSortIndicator(int column, bool ascending)
    {
        SortColumn = column;
        SortAscending = ascending;
        QueueRedraw();
    }

    public void ScrollToRow(int row)
    {
        var rh = RowH;
        var top = row * rh;
        if (top < _v.Value) _v.Value = top;
        else if (top + rh > _v.Value + Body.Size.Y) _v.Value = top + rh - Body.Size.Y;
    }

    // ---- metrics -------------------------------------------------------------------------

    int RowH => Math.Max(16, GetThemeConstant("row_height", "DataGrid"));
    int HeaderH => ShowHeader ? GetThemeConstant("header_height", "DataGrid") : 0;
    int FooterH => ShowFooter ? GetThemeConstant("footer_height", "DataGrid") : 0;
    int Pad => GetThemeConstant("cell_padding", "DataGrid");
    float VBarW => _v.Visible ? _v.GetCombinedMinimumSize().X : 0;
    float HBarH => _h.Visible ? _h.GetCombinedMinimumSize().Y : 0;

    /// <summary>The rows region, inside the 1px frame.</summary>
    Rect2 Body => new(1, 1 + HeaderH, Size.X - 2 - VBarW, Size.Y - 2 - HeaderH - FooterH - HBarH);

    float ContentWidth()
    {
        float w = 0;
        foreach (var c in Columns) w += c.Width;
        return w;
    }

    /// <summary>Column widths as drawn: an Expand column absorbs leftover space.</summary>
    float[] DrawnWidths()
    {
        var widths = new float[Columns.Count];
        float total = 0;
        for (int i = 0; i < widths.Length; i++) { widths[i] = Columns[i].Width; total += widths[i]; }
        var spare = Body.Size.X - total;
        if (spare > 0)
            for (int i = 0; i < widths.Length; i++)
                if (Columns[i].Expand) { widths[i] += spare; break; }
        return widths;
    }

    void Relayout()
    {
        if (!IsInsideTree()) return;
        var content = (float)_rowCount * RowH;
        // Decide bar visibility twice: each bar steals room from the other.
        for (int pass = 0; pass < 2; pass++)
        {
            _v.Visible = content > Size.Y - 2 - HeaderH - FooterH - HBarH;
            _h.Visible = ContentWidth() > Size.X - 2 - VBarW;
        }
        var body = Body;
        _v.Position = new Vector2(Size.X - 1 - VBarW, body.Position.Y);
        _v.Size = new Vector2(VBarW, body.Size.Y);
        _v.MaxValue = content;
        _v.Page = body.Size.Y;
        _v.Step = 1;
        _h.Position = new Vector2(1, body.End.Y);
        _h.Size = new Vector2(body.Size.X, HBarH);
        _h.MaxValue = ContentWidth();
        _h.Page = body.Size.X;
        OnScrolled();
    }

    void OnScrolled()
    {
        QueueRedraw();
        var (first, last) = VisibleRange();
        var range = (first, last, _rowCount);
        if (range == _lastRange) return;
        _lastRange = range;
        EmitSignal(SignalName.VisibleRangeChanged, first, last, _rowCount);
    }

    /// <summary>1-based first and last row fully in view (a row cut by the footer is not named); (0, 0) when empty.</summary>
    public (int First, int Last) VisibleRange()
    {
        if (_rowCount == 0) return (0, 0);
        var rh = RowH;
        var first = (int)Math.Ceiling(_v.Value / rh - 0.001);
        var last = (int)Math.Floor((_v.Value + Body.Size.Y + 0.5) / rh) - 1; // half a pixel of tolerance for fractional layout
        first = Math.Min(first, _rowCount - 1);
        return (first + 1, Math.Clamp(last, first, _rowCount - 1) + 1);
    }

    public override void _Notification(int what)
    {
        if (what == NotificationResized || what == NotificationThemeChanged || what == NotificationEnterTree)
            Relayout();
        if (what == NotificationMouseExit) { _hoverRow = -1; QueueRedraw(); }
        if (what == NotificationFocusEnter || what == NotificationFocusExit) QueueRedraw();
    }

    // ---- drawing -------------------------------------------------------------------------

    public override void _Draw()
    {
        const string T = "DataGrid";
        var font = GetThemeFont("font", T);
        var bold = GetThemeFont("header_font", T);
        var fs = GetThemeFontSize("font_size", T);
        var small = GetThemeFontSize("small_font_size", T);
        var ink = GetThemeColor("font_color", T);
        var muted = GetThemeColor("font_muted_color", T);
        var rule = GetThemeColor("rule_color", T);
        var frame = GetThemeColor("frame_color", T);
        var pad = Pad;
        var rh = RowH;
        var body = Body;
        var widths = DrawnWidths();
        var xOff = -(float)_h.Value;

        DrawStyleBox(GetThemeStylebox("panel", T), new Rect2(Vector2.Zero, Size));

        // Rows
        if (_rowCount == 0)
        {
            DrawText(EmptyText, font, fs, muted, body, HorizontalAlignment.Center);
        }
        else
        {
            var first = (int)(_v.Value / rh);
            var y = body.Position.Y - (float)(_v.Value - first * rh);
            for (int r = first; r < _rowCount && y < body.End.Y; r++, y += rh)
            {
                var rowRect = new Rect2(body.Position.X, y, body.Size.X, rh);
                if (r == _selected) DrawRect(rowRect, GetThemeColor("selected_color", T));
                else if (r == _hoverRow) DrawRect(rowRect, GetThemeColor("hover_color", T));
                var x = body.Position.X + xOff;
                for (int c = 0; c < Columns.Count; c++)
                {
                    var col = Columns[c];
                    var text = CellText?.Invoke(r, c) ?? "";
                    var cell = new Rect2(x + pad, y, widths[c] - 2 * pad, rh);
                    DrawText(text, font, fs, ink, cell, col.Numeric ? HorizontalAlignment.Right : HorizontalAlignment.Left);
                    x += widths[c];
                }
                DrawLine(new Vector2(body.Position.X, y + rh - 0.5f), new Vector2(body.End.X, y + rh - 0.5f), rule);
                if (r == _selected && HasFocus())
                    DrawRect(new Rect2(rowRect.Position + new Vector2(0.5f, 0.5f), rowRect.Size - Vector2.One), GetThemeColor("cursor_color", T), false, 1);
            }
        }

        // Column rules run through header and body: a ruled table, not stripes.
        {
            var x = body.Position.X + xOff;
            for (int c = 0; c < Columns.Count - 1; c++)
            {
                x += widths[c];
                if (x > body.Position.X && x < body.End.X)
                    DrawLine(new Vector2(MathF.Round(x) - 0.5f, 1), new Vector2(MathF.Round(x) - 0.5f, body.End.Y), rule);
            }
        }

        // Header
        if (ShowHeader)
        {
            var hdr = new Rect2(1, 1, Size.X - 2, HeaderH);
            DrawRect(hdr, GetThemeColor("header_color", T));
            var x = body.Position.X + xOff;
            for (int c = 0; c < Columns.Count; c++)
            {
                var col = Columns[c];
                var w = widths[c];
                var iconW = 0f;
                if (c == SortColumn)
                {
                    var icon = GetThemeIcon(SortAscending ? "sort_asc" : "sort_desc", T);
                    iconW = icon.GetWidth() + 4;
                    var ix = col.Numeric ? x + pad : x + w - pad - icon.GetWidth();
                    DrawTextureRect(icon, new Rect2(ix, hdr.Position.Y + (HeaderH - 16) / 2f, 16, 16), false);
                }
                var cell = col.Numeric
                    ? new Rect2(x + pad + iconW, hdr.Position.Y, w - 2 * pad - iconW, HeaderH)
                    : new Rect2(x + pad, hdr.Position.Y, w - 2 * pad - iconW, HeaderH);
                DrawText(col.Title, bold, fs, ink, cell, col.Numeric ? HorizontalAlignment.Right : HorizontalAlignment.Left);
                if (c != Columns.Count - 1)
                    DrawLine(new Vector2(MathF.Round(x + w) - 0.5f, 1), new Vector2(MathF.Round(x + w) - 0.5f, HeaderH + 1), frame);
                x += w;
            }
            DrawLine(new Vector2(1, HeaderH + 0.5f), new Vector2(Size.X - 1, HeaderH + 0.5f), frame);

            // Dimension line while a column is being resized: the width, measured like a drawing.
            if (_resizing >= 0)
            {
                var accent = GetThemeColor("accent_color", T);
                float cx = body.Position.X + xOff;
                for (int c = 0; c < _resizing; c++) cx += widths[c];
                var cw = widths[_resizing];
                var ly = HeaderH - 5.5f;
                DrawLine(new Vector2(cx + 2, ly), new Vector2(cx + cw - 2, ly), accent);
                DrawLine(new Vector2(cx + 2.5f, ly - 4), new Vector2(cx + 2.5f, ly + 4), accent);
                DrawLine(new Vector2(cx + cw - 2.5f, ly - 4), new Vector2(cx + cw - 2.5f, ly + 4), accent);
                var label = $"{Mathf.RoundToInt(Columns[_resizing].Width)} px";
                var lw = bold.GetStringSize(label, HorizontalAlignment.Left, -1, small).X + 8;
                var lr = new Rect2(cx + (cw - lw) / 2, 2, lw, HeaderH - 10);
                DrawRect(lr, GetThemeColor("header_color", T));
                DrawText(label, bold, small, accent, lr, HorizontalAlignment.Center);
            }
        }

        // Footer: the guide words.
        if (ShowFooter)
        {
            var fy = Size.Y - 1 - FooterH;
            var foot = new Rect2(1, fy, Size.X - 2, FooterH);
            DrawRect(foot, GetThemeColor("header_color", T));
            DrawLine(new Vector2(1, fy + 0.5f), new Vector2(Size.X - 1, fy + 0.5f), frame);
            var (a, b) = VisibleRange();
            var inv = CultureInfo.InvariantCulture;
            var words = string.Format(RangeFormat, a.ToString("N0", inv), b.ToString("N0", inv), _rowCount.ToString("N0", inv));
            DrawText(words, font, small, muted, new Rect2(foot.Position.X + pad, fy, foot.Size.X - 2 * pad, FooterH), HorizontalAlignment.Left);
            if (_selected >= 0)
                DrawText($"Selected {(_selected + 1).ToString("N0", inv)}", font, small, muted,
                    new Rect2(foot.Position.X + pad, fy, foot.Size.X - 2 * pad, FooterH), HorizontalAlignment.Right);
        }

        if (HasFocus())
            DrawStyleBox(GetThemeStylebox("focus", T), new Rect2(Vector2.Zero, Size));
    }

    // ponytail: a TextLine per visible cell per redraw; cache shaped lines per (row, col) if profiling ever shows it.
    void DrawText(string text, Font font, int size, Color color, Rect2 box, HorizontalAlignment align)
    {
        if (box.Size.X <= 4 || string.IsNullOrEmpty(text)) return;
        var line = new TextLine
        {
            Width = box.Size.X,
            Alignment = align,
            TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis,
        };
        line.AddString(text, font, size);
        var y = box.Position.Y + (box.Size.Y - line.GetSize().Y) / 2;
        line.Draw(GetCanvasItem(), new Vector2(box.Position.X, MathF.Round(y)), color);
    }

    // ---- input ---------------------------------------------------------------------------

    int ColumnEdgeAt(float x)
    {
        var widths = DrawnWidths();
        var edge = Body.Position.X - (float)_h.Value;
        for (int c = 0; c < widths.Length; c++)
        {
            edge += widths[c];
            if (Math.Abs(x - edge) <= 4) return c;
        }
        return -1;
    }

    int ColumnAt(float x)
    {
        var widths = DrawnWidths();
        var left = Body.Position.X - (float)_h.Value;
        for (int c = 0; c < widths.Length; c++)
        {
            if (x >= left && x < left + widths[c]) return c;
            left += widths[c];
        }
        return -1;
    }

    int RowAt(float y)
    {
        var body = Body;
        if (y < body.Position.Y || y >= body.End.Y) return -1;
        var r = (int)((y - body.Position.Y + _v.Value) / RowH);
        return r < _rowCount ? r : -1;
    }

    public override void _GuiInput(InputEvent e)
    {
        switch (e)
        {
            case InputEventMouseMotion m:
                if (_resizing >= 0)
                {
                    var col = Columns[_resizing];
                    col.Width = Math.Max(col.MinWidth, _resizeStartW + m.Position.X - _resizeStartX);
                    Relayout();
                    AcceptEvent();
                    return;
                }
                var inHeader = ShowHeader && m.Position.Y < HeaderH + 1;
                MouseDefaultCursorShape = inHeader && ColumnEdgeAt(m.Position.X) >= 0 ? CursorShape.Hsize : CursorShape.Arrow;
                var hr = RowAt(m.Position.Y);
                if (hr != _hoverRow) { _hoverRow = hr; QueueRedraw(); }
                break;

            case InputEventMouseButton b when b.ButtonIndex == MouseButton.Left:
                if (!b.Pressed)
                {
                    if (_resizing >= 0) { _resizing = -1; QueueRedraw(); AcceptEvent(); }
                    return;
                }
                GrabFocus();
                if (ShowHeader && b.Position.Y < HeaderH + 1)
                {
                    var edge = ColumnEdgeAt(b.Position.X);
                    if (edge >= 0)
                    {
                        _resizing = edge;
                        _resizeStartX = b.Position.X;
                        _resizeStartW = Columns[edge].Width;
                        QueueRedraw();
                    }
                    else
                    {
                        var c = ColumnAt(b.Position.X);
                        if (c >= 0 && Columns[c].Sortable)
                        {
                            var asc = c != SortColumn || !SortAscending;
                            SetSortIndicator(c, asc);
                            EmitSignal(SignalName.SortRequested, c, asc);
                        }
                    }
                    AcceptEvent();
                    return;
                }
                var row = RowAt(b.Position.Y);
                if (row >= 0)
                {
                    SelectedRow = row;
                    if (b.DoubleClick) EmitSignal(SignalName.RowActivated, row);
                }
                AcceptEvent();
                break;

            case InputEventMouseButton w when w.Pressed && w.ButtonIndex is MouseButton.WheelUp or MouseButton.WheelDown:
                var dir = w.ButtonIndex == MouseButton.WheelDown ? 1 : -1;
                if (w.ShiftPressed) _h.Value += dir * 48 * w.Factor;
                else _v.Value += dir * RowH * 3 * w.Factor;
                AcceptEvent();
                break;

            case InputEventMouseButton w when w.Pressed && w.ButtonIndex is MouseButton.WheelLeft or MouseButton.WheelRight:
                _h.Value += (w.ButtonIndex == MouseButton.WheelRight ? 1 : -1) * 48 * w.Factor;
                AcceptEvent();
                break;

            case InputEventPanGesture p:
                _v.Value += p.Delta.Y * RowH;
                _h.Value += p.Delta.X * 24;
                AcceptEvent();
                break;

            case InputEventKey { Pressed: true } when _rowCount > 0:
                var page = Math.Max(1, (int)(Body.Size.Y / RowH) - 1);
                int? target =
                    e.IsAction("ui_up", true) ? Math.Max(0, _selected - 1) :
                    e.IsAction("ui_down", true) ? Math.Min(_rowCount - 1, _selected + 1) :
                    e.IsAction("ui_page_up", true) ? Math.Max(0, _selected - page) :
                    e.IsAction("ui_page_down", true) ? Math.Min(_rowCount - 1, Math.Max(0, _selected) + page) :
                    e.IsAction("ui_home", true) ? 0 :
                    e.IsAction("ui_end", true) ? _rowCount - 1 : null;
                if (target is int t)
                {
                    SelectedRow = t;
                    ScrollToRow(t);
                    AcceptEvent();
                }
                else if (e.IsAction("ui_accept") && _selected >= 0)
                {
                    EmitSignal(SignalName.RowActivated, _selected);
                    AcceptEvent();
                }
                else if (e.IsAction("ui_left", true) || e.IsAction("ui_right", true))
                {
                    _h.Value += (e.IsAction("ui_right", true) ? 1 : -1) * 48;
                    AcceptEvent();
                }
                break;
        }
    }
}

/// <summary>A virtualized single-column list: a <see cref="DataGrid"/> without header or rules between columns.</summary>
[GlobalClass]
public partial class ListView : DataGrid
{
    IReadOnlyList<string> _items = Array.Empty<string>();

    public ListView()
    {
        ShowHeader = false;
        ShowFooter = false;
        Columns.Add(new GridColumn { Expand = true, Width = 0, MinWidth = 0, Sortable = false });
        CellText = (r, _) => _items[r];
    }

    public IReadOnlyList<string> Items
    {
        get => _items;
        set { _items = value; RowCount = value.Count; }
    }
}
