using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Godot;

namespace Gabarit.Gallery;

/// <summary>
/// The Gallery: every component as a clause of a specification sheet.
/// Command-line (after --): clause=2.1  sheet=slate  shot=/path.png  (screenshot then quit)
/// </summary>
public partial class Main : Control
{
    record Clause(string Number, string Title, string Class, string Rule, Func<Control> Specimen,
        (string State, string Mark, string Keys)[] States, string Usage);

    const string Version = "0.1.0";
    static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    Tokens _tokens = Tokens.CreatePaper();
    bool _slate;
    Clause[] _clauses = [];
    Clause? _current;

    readonly Tree _index = new();
    readonly ScrollContainer _scroll = new();
    readonly VBoxContainer _sheet = new();
    readonly HBoxContainer _titleBlock = new();

    public override void _Ready()
    {
        var args = ParseArgs();
        // Window sizes are in physical pixels on HiDPI screens: grow the window with the UI scale.
        var scale = args.TryGetValue("scale", out var sc) ? float.Parse(sc, Inv) : DisplayServer.ScreenGetScale();
        var win = GetWindow();
        win.ContentScaleFactor = scale;
        win.MinSize = (Vector2I)(new Vector2(960, 600) * scale);
        if (scale != 1 && !args.ContainsKey("shot"))
        {
            win.Size = (Vector2I)(new Vector2(1440, 900) * scale);
            win.MoveToCenter();
        }
        _slate = args.GetValueOrDefault("sheet") == "slate";
        _tokens = _slate ? Tokens.CreateSlate() : Tokens.CreatePaper();
        Theme = ThemeBuilder.Build(_tokens);
        _clauses = BuildClauses();
        BuildShell();
        var start = args.GetValueOrDefault("clause", "2.1");
        Show(_clauses.FirstOrDefault(c => c.Number == start) ?? _clauses[0]);

        if (args.ContainsKey("focus")) Callable.From(FocusSpecimen).CallDeferred();
        if (args.ContainsKey("resize")) _ = ResizeDemo();
        if (args.TryGetValue("shot", out var shot)) _ = Screenshot(shot);
    }

    // ---- shell -----------------------------------------------------------------------------

    void BuildShell()
    {
        var ground = new PanelContainer { ThemeTypeVariation = "GroundPanel" };
        ground.SetAnchorsPreset(LayoutPreset.FullRect);
        AddChild(ground);

        var split = new HBoxContainer();
        split.AddThemeConstantOverride("separation", 0);
        ground.AddChild(split);

        // Index: the specification's table of contents.
        var left = new PanelContainer { CustomMinimumSize = new Vector2(264, 0) };
        split.AddChild(left);
        var leftCol = new VBoxContainer();
        leftCol.AddThemeConstantOverride("separation", 0);
        left.AddChild(leftCol);

        var mast = Margin(new VBoxContainer(), 20, 20, 20, 16);
        var mastCol = (VBoxContainer)mast.GetChild(0);
        mastCol.AddThemeConstantOverride("separation", 2);
        mastCol.AddChild(new Label { Text = "Gabarit", ThemeTypeVariation = "HeadingLabel" });
        mastCol.AddChild(new Label { Text = "Desktop UI components for Godot", ThemeTypeVariation = "CaptionLabel", AutowrapMode = TextServer.AutowrapMode.WordSmart });
        leftCol.AddChild(mast);
        leftCol.AddChild(new HSeparator());

        _index.HideRoot = true;
        _index.SizeFlagsVertical = SizeFlags.ExpandFill;
        _index.AddThemeStyleboxOverride("panel", new StyleBoxEmpty { ContentMarginTop = 8, ContentMarginBottom = 8, ContentMarginLeft = 8, ContentMarginRight = 8 });
        _index.AddThemeConstantOverride("draw_relationship_lines", 0);
        _index.AddThemeConstantOverride("v_separation", 6);
        IndexStyle();
        _index.ItemSelected += () =>
        {
            if (_index.GetSelected()?.GetMetadata(0).AsString() is { Length: > 0 } n && _clauses.FirstOrDefault(c => c.Number == n) is { } cl && cl != _current)
                Show(cl);
        };
        leftCol.AddChild(_index);
        FillIndex();

        var right = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        right.AddThemeConstantOverride("separation", 0);
        split.AddChild(new VSeparator());
        split.AddChild(right);

        _scroll.SizeFlagsVertical = SizeFlags.ExpandFill;
        _scroll.HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled;
        right.AddChild(_scroll);
        _sheet.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _sheet.AddThemeConstantOverride("separation", 0);
        _scroll.AddChild(Margin(_sheet, 48, 40, 48, 48));

        right.AddChild(new HSeparator());
        var bottom = Margin(_titleBlock, 24, 12, 24, 12);
        var band = new PanelContainer();
        band.AddChild(bottom);
        right.AddChild(band);
    }

    /// <summary>The index is ruled like a table of contents; the current clause is marked in ink, not tint.</summary>
    void IndexStyle()
    {
        var mark = new StyleBoxFlat { DrawCenter = false, BorderColor = _tokens.Ink };
        mark.SetBorderWidthAll(1);
        foreach (var n in new[] { "selected", "selected_focus", "hovered_selected", "hovered_selected_focus" })
            _index.AddThemeStyleboxOverride(n, mark);
        _index.AddThemeConstantOverride("draw_guides", 1);
    }

    void FillIndex()
    {
        _index.Clear();
        var root = _index.CreateItem();
        TreeItem? section = null;
        foreach (var c in _clauses)
        {
            var major = c.Number.Split('.')[0];
            if (section?.GetMetadata(0).AsString() != "§" + major)
            {
                section = _index.CreateItem(root);
                section.SetText(0, major == "1" ? "1   Foundations" : "2   Data");
                section.SetMetadata(0, "§" + major);
                section.SetSelectable(0, false);
                section.SetCustomFont(0, _tokens.Bold);
                section.SetCustomFontSize(0, _tokens.SmallSize);
                section.SetCustomColor(0, _tokens.InkMuted);
            }
            var item = _index.CreateItem(section);
            item.SetText(0, $"{c.Number}   {c.Title}");
            item.SetMetadata(0, c.Number);
        }
    }

    void Show(Clause c)
    {
        _current = c;
        foreach (var child in _sheet.GetChildren()) child.QueueFree();

        // Select the clause in the index without re-entering Show.
        for (var s = _index.GetRoot()?.GetFirstChild(); s != null; s = s.GetNext())
            for (var i = s.GetFirstChild(); i != null; i = i.GetNext())
            {
                var on = i.GetMetadata(0).AsString() == c.Number;
                i.SetCustomFont(0, on ? _tokens.Bold : null);
                if (on && !i.IsSelected(0)) { i.Select(0); _index.ScrollToItem(i); }
            }

        var head = new HBoxContainer();
        head.AddThemeConstantOverride("separation", 16);
        var num = new Label { Text = c.Number, ThemeTypeVariation = "TitleLabel" };
        num.AddThemeColorOverride("font_color", _tokens.InkMuted);
        head.AddChild(num);
        head.AddChild(new Label { Text = c.Title, ThemeTypeVariation = "TitleLabel" });
        _sheet.AddChild(head);
        _sheet.AddChild(Gap(8));
        _sheet.AddChild(Prose(c.Rule));
        _sheet.AddChild(Gap(28));

        var frame = new PanelContainer { ThemeTypeVariation = "SheetPanel" };
        frame.AddChild(Margin(c.Specimen(), 28, 28, 28, 28));
        _sheet.AddChild(frame);

        _sheet.AddChild(Gap(40));
        _sheet.AddChild(new Label { Text = "States", ThemeTypeVariation = "HeadingLabel" });
        _sheet.AddChild(Gap(12));
        _sheet.AddChild(RuledTable(["State", "Mark", "Keyboard"], c.States.Select(s => new[] { s.State, s.Mark, s.Keys }), [160, 0, 220]));

        _sheet.AddChild(Gap(40));
        _sheet.AddChild(new Label { Text = "Usage", ThemeTypeVariation = "HeadingLabel" });
        _sheet.AddChild(Gap(12));
        _sheet.AddChild(Code(c.Usage));

        BuildTitleBlock(c);
        _scroll.ScrollVertical = 0;
    }

    void BuildTitleBlock(Clause c)
    {
        foreach (var child in _titleBlock.GetChildren()) child.QueueFree();
        var hint = new Label
        {
            Text = "Tab moves through the specimen  ·  Up / Down in the index changes clause",
            ThemeTypeVariation = "CaptionLabel",
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            VerticalAlignment = VerticalAlignment.Center,
            TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis,
        };
        _titleBlock.AddChild(hint);

        var sheet = new OptionButton { TooltipText = "Sheet: switches every token at once" };
        sheet.AddItem("Paper");
        sheet.AddItem("Slate");
        sheet.Select(_slate ? 1 : 0);
        sheet.ItemSelected += i => SetSheet(i == 1);

        var cells = new (string Key, Control Value)[]
        {
            ("Component", new Label { Text = c.Title, ThemeTypeVariation = "StrongLabel" }),
            ("Class", new Label { Text = c.Class }),
            ("Version", new Label { Text = Version }),
            ("Page", new Label { Text = $"{Array.IndexOf(_clauses, c) + 1} of {_clauses.Length}" }),
            ("Sheet", sheet),
        };
        var block = new HBoxContainer();
        block.AddThemeConstantOverride("separation", 0);
        foreach (var (key, value) in cells)
        {
            var col = new VBoxContainer();
            col.AddThemeConstantOverride("separation", 2);
            col.AddChild(new Label { Text = key, ThemeTypeVariation = "CaptionLabel" });
            value.SizeFlagsVertical = SizeFlags.ShrinkCenter;
            col.AddChild(value);
            block.AddChild(Cell(Margin(col, 12, 6, 16, 6), Colors.Transparent, left: key != "Component"));
        }
        var outer = new PanelContainer();
        outer.AddThemeStyleboxOverride("panel", Rule(_tokens.Paper, _tokens.Rule, 1, 1, 1, 1));
        outer.AddChild(block);
        _titleBlock.AddChild(outer);
    }

    void SetSheet(bool slate)
    {
        _slate = slate;
        _tokens = slate ? Tokens.CreateSlate() : Tokens.CreatePaper();
        Theme = ThemeBuilder.Build(_tokens);
        IndexStyle();
        FillIndex();
        Callable.From(() => { if (_current is not null) Show(_current); }).CallDeferred();
    }

    // ---- clauses ---------------------------------------------------------------------------

    Clause[] BuildClauses() =>
    [
        new("1.1", "Tokens", "Gabarit.Tokens",
            "Every colour, face and metric is a token on one Resource; the theme is built from nothing else.",
            TokensSpecimen,
            [("Paper", "white sheet, black ink", "—"), ("Slate", "same names, inverted values", "—"), ("Custom", "any .tres you save from Tokens", "—")],
            "var tokens = Tokens.CreatePaper();\ntokens.Action = new Color(\"#2f7d32\");\nGetTree().Root.GetChild<Control>(0).Theme = ThemeBuilder.Build(tokens);"),

        new("1.2", "Button", "Button  ·  PrimaryButton  ·  FlatButton",
            "A button names its action; one primary button per view carries the action colour.",
            ButtonSpecimen,
            [("Rest", "1px graphite rule", "—"), ("Hover", "band fill, ink rule", "—"), ("Pressed", "filled ink, paper text", "Space · Enter"),
             ("Focus", "2px ink frame, 2px outside", "Tab · Shift+Tab"), ("Disabled", "dashed rule, muted text", "—")],
            "var save = new Button { Text = \"Save\" };\nvar export = new Button { Text = \"Export\", ThemeTypeVariation = \"PrimaryButton\" };"),

        new("1.3", "Text field", "Gabarit.Field",
            "Label above, input, then one line for help or error in a fixed slot, so the form never jumps.",
            FieldSpecimen,
            [("Rest", "1px graphite rule", "—"), ("Focus", "2px ink rule, caret 2px", "Tab"), ("Error", "danger rule, message below", "editing clears it"),
             ("Read-only", "dashed rule on ground", "skipped by Tab"), ("Placeholder", "muted ink, 4.5:1", "—")],
            "var field = new Field { Label = \"Output folder\", Help = \"Relative to the project.\" };\nfield.Error = \"Folder does not exist.\";"),

        new("1.4", "Checkbox", "CheckBox",
            "An independent yes/no option: the square carries the state, the label never changes colour.",
            CheckboxSpecimen,
            [("Off", "empty square, graphite rule", "Space"), ("On", "filled ink square, paper check", "Space"),
             ("Focus", "2px ink frame", "Tab"), ("Disabled", "dashed square", "—")],
            "var box = new CheckBox { Text = \"Overwrite existing files\" };\nbox.Toggled += on => GD.Print(on);"),

        new("1.5", "Switch", "CheckButton",
            "A setting that takes effect immediately; use a checkbox (see 1.4) when the choice waits for Save.",
            SwitchSpecimen,
            [("Off", "carriage left, open slot", "Space"), ("On", "carriage right, action slot", "Space"),
             ("Focus", "2px ink frame", "Tab"), ("Disabled", "dashed slot", "—")],
            "var autosave = new CheckButton { Text = \"Auto-save\", ButtonPressed = true };"),

        new("1.6", "Select", "OptionButton",
            "One choice from a short, known list; beyond a dozen options, use a ListView with a filter (see 2.3).",
            SelectSpecimen,
            [("Rest", "graphite rule, chevron", "—"), ("Open", "band fill, popup with shadow", "Space · Enter"),
             ("Item hover", "action tint", "Up · Down"), ("Disabled", "dashed rule", "—")],
            "var format = new OptionButton();\nformat.AddItem(\"PNG\");\nformat.AddItem(\"WebP\");\nformat.ItemSelected += i => GD.Print(i);"),

        new("1.7", "Slider", "HSlider",
            "A value on a bounded scale: the ink length is the value, exactly, and a numeric field states it.",
            SliderSpecimen,
            [("Rest", "ink up to the reading, index mark", "—"), ("Hover", "mark and fill take the action colour", "—"),
             ("Focus", "2px ink frame", "Left · Right"), ("Disabled", "graphite mark", "—")],
            "var opacity = new HSlider { MinValue = 0, MaxValue = 100, Step = 1, Value = 80 };"),

        new("1.8", "Type", "Label variations",
            "One family, D-DIN, in two weights with tabular figures; hierarchy comes from size and weight, never colour.",
            TypeSpecimen,
            [("TitleLabel", $"bold {_tokens.TitleSize}px", "—"), ("HeadingLabel", $"bold {_tokens.FontSize + 4}px", "—"), ("Label", $"regular {_tokens.FontSize}px", "—"),
             ("StrongLabel", $"bold {_tokens.FontSize}px", "—"), ("CaptionLabel", $"regular {_tokens.SmallSize}px, muted ink", "—"), ("FieldLabel", $"bold {_tokens.SmallSize}px", "—")],
            "var title = new Label { Text = \"Export presets\", ThemeTypeVariation = \"TitleLabel\" };"),

        new("1.9", "Icon", "Gabarit.Icon",
            "Authored 16px drawings, one 1.5px square-capped stroke, rendered from SVG at the screen's density.",
            IconSpecimen,
            [("Default", "theme ink", "—"), ("Tinted", "any token via Tint", "—"), ("In a button", "follows the button's text colour", "—")],
            "var icon = new Icon { IconName = \"download\" };\nvar button = new Button { Text = \"Export\", Icon = Icons.Get(\"download\", tokens.Ink) };"),

        new("2.1", "DataGrid", "Gabarit.DataGrid",
            "A virtualized, fully ruled table: only visible rows are drawn, and the footer names them.",
            GridSpecimen,
            [("Row hover", "ground fill", "—"), ("Row selected", "action tint", "Up · Down · Page · Home · End"), ("Row activated", "RowActivated signal", "Enter · double-click"),
             ("Sorted", "arrow beside the title", "click header"), ("Resizing", "dimension line with width in px", "drag a header rule"), ("Empty", "“No rows” centred", "—")],
            "grid.SetData(columns, rows.Count, (r, c) => rows[r][c]);\ngrid.SortRequested += (col, asc) => { Sort(col, asc); grid.QueueRedraw(); };"),

        new("2.2", "TreeView", "Tree",
            "Hierarchies with a ruled frame and right-angled guides: the native Tree, themed.",
            TreeSpecimen,
            [("Collapsed", "chevron right", "Right · Left"), ("Selected", "action tint", "Up · Down"), ("Focus", "2px ink frame", "Tab"), ("Column title", "band, bold", "—")],
            "var tree = new Tree { Columns = 2, ColumnTitlesVisible = true };\nvar root = tree.CreateItem();"),

        new("2.3", "ListView", "Gabarit.ListView",
            "A virtualized single-column list for long, uniform items such as logs, with the same guide words as 2.1.",
            ListSpecimen,
            [("Hover", "ground fill", "—"), ("Selected", "action tint", "Up · Down · Page · Home · End"), ("Activated", "RowActivated signal", "Enter")],
            "var log = new ListView { Items = lines };"),

        new("2.4", "PropertyInspector", "Gabarit.PropertyInspector",
            "Edits the exported properties of any object, with an editor chosen by each property's type and hint.",
            InspectorSpecimen,
            [("Group", "band row, bold", "—"), ("Edited", "PropertyEdited signal", "Enter commits text"), ("Unsupported type", "value shown read-only", "—")],
            "var inspector = new PropertyInspector();\ninspector.Inspect(myResource);\ninspector.PropertyEdited += name => Save();"),
    ];

    // ---- specimens -------------------------------------------------------------------------

    Control TokensSpecimen()
    {
        var t = _tokens;
        var rows = new (string Name, Color Color, string Use)[]
        {
            ("Ground", t.Ground, "application background"), ("Paper", t.Paper, "sheets, controls"), ("Band", t.Band, "header bands"),
            ("Ink", t.Ink, "text"), ("InkMuted", t.InkMuted, "secondary text"), ("Rule", t.Rule, "control outlines"),
            ("Hairline", t.Hairline, "table rules"), ("Action", t.Action, "actionable, selected"), ("ActionTint", t.ActionTint, "selection fill"),
            ("OnAction", t.OnAction, "text on action"), ("Danger", t.Danger, "errors"),
        };
        var others = new (string Name, string Value, string Use)[]
        {
            ("Regular", "D-DIN + Gabarit Figures", "body, tables"), ("Bold", "D-DIN Bold + Gabarit Figures", "titles, headers"),
            ("FontSize", $"{t.FontSize} px", "body"), ("SmallSize", $"{t.SmallSize} px", "captions, labels"), ("TitleSize", $"{t.TitleSize} px", "clause titles"),
            ("ControlHeight", $"{t.ControlHeight} px", "buttons, fields, selects"), ("RowHeight", $"{t.RowHeight} px", "grid and list rows"),
            ("Unit", $"{t.Unit} px", "spacing grid"), ("Radius", $"{t.Radius} px", "corners, maximum"),
        };
        var table = RuledTable(["Token", "Swatch", "Value", "Use", "On Paper"],
            rows.Select(r => new[] { r.Name, "", "#" + r.Color.ToHtml(false).ToUpperInvariant(), r.Use, Contrast(r.Color, t.Paper) })
                .Concat(others.Select(o => new[] { o.Name, "", o.Value, o.Use, "—" })),
            [140, 72, 190, 0, 110], numeric: [false, false, false, false, true],
            decorate: (row, col, cell) =>
            {
                if (col != 1 || row >= rows.Length) return;
                var chip = new Panel { CustomMinimumSize = new Vector2(40, 16) };
                chip.AddThemeStyleboxOverride("panel", Rule(rows[row].Color, t.Rule, 1, 1, 1, 1));
                cell.AddChild(chip);
            });
        return table;
    }

    Control ButtonSpecimen()
    {
        var col = Column(20);
        var row = Row(12);
        row.AddChild(new Button { Text = "Export", ThemeTypeVariation = "PrimaryButton", Icon = Icons.Get("download", _tokens.OnAction) });
        row.AddChild(new Button { Text = "Save" });
        row.AddChild(new Button { Text = "Revert", Icon = Icons.Get("refresh", _tokens.Ink) });
        row.AddChild(new Button { Text = "Delete preset", Disabled = true });
        col.AddChild(row);

        var bar = Row(4);
        foreach (var (icon, tip) in new[] { ("plus", "New preset"), ("filter", "Filter"), ("refresh", "Reload"), ("settings", "Settings") })
            bar.AddChild(new Button { Icon = Icons.Get(icon, _tokens.Ink), ThemeTypeVariation = "FlatButton", TooltipText = tip });
        bar.AddChild(new VSeparator());
        bar.AddChild(new Button { Text = "Toggle", ThemeTypeVariation = "FlatButton", ToggleMode = true, ButtonPressed = true, TooltipText = "Toggle mode: pressed is filled ink" });
        col.AddChild(Labelled("Toolbar, flat", bar));
        return col;
    }

    Control FieldSpecimen()
    {
        var grid = new GridContainer { Columns = 2 };
        grid.AddThemeConstantOverride("h_separation", 32);
        grid.AddThemeConstantOverride("v_separation", 20);
        var w = new Vector2(320, 0);
        grid.AddChild(new Field { Label = "Preset name", Text = "Release — desktop", Help = "Shown in the export menu.", CustomMinimumSize = w });
        var output = new Field { Label = "Output folder", Text = "build/out/win64", CustomMinimumSize = w };
        output.Error = "Folder does not exist. Create it or choose another.";
        grid.AddChild(output);
        grid.AddChild(new Field { Label = "Assembly", Text = "Gabarit.dll", Help = "Set by the project; read-only.", Editable = false, CustomMinimumSize = w });
        var search = new Field { Label = "Filter", Placeholder = "Type to filter rows", CustomMinimumSize = w };
        search.Input.RightIcon = Icons.Get("search", _tokens.InkMuted);
        grid.AddChild(search);
        return grid;
    }

    Control CheckboxSpecimen()
    {
        var col = Column(12);
        col.AddChild(new CheckBox { Text = "Include hidden files", ButtonPressed = true });
        col.AddChild(new CheckBox { Text = "Overwrite existing files" });
        col.AddChild(new CheckBox { Text = "Sign assemblies (set by policy)", ButtonPressed = true, Disabled = true });
        col.AddChild(new CheckBox { Text = "Upload symbols (not configured)", Disabled = true });
        return col;
    }

    Control SwitchSpecimen()
    {
        // CheckButton puts its switch at the far end: a settings column, not the full sheet width.
        var col = Column(12);
        col.CustomMinimumSize = new Vector2(360, 0);
        col.SizeFlagsHorizontal = SizeFlags.ShrinkBegin;
        col.AddChild(new CheckButton { Text = "Auto-save", ButtonPressed = true });
        col.AddChild(new CheckButton { Text = "Snap to grid" });
        col.AddChild(new CheckButton { Text = "Network sync (offline)", Disabled = true });
        col.AddChild(new CheckButton { Text = "Crash reports (managed by IT)", ButtonPressed = true, Disabled = true });
        return col;
    }

    Control SelectSpecimen()
    {
        var row = Row(32);
        var format = new OptionButton { CustomMinimumSize = new Vector2(200, 0) };
        foreach (var f in new[] { "PNG", "WebP", "JPEG" }) format.AddItem(f);
        format.Select(1);
        row.AddChild(Labelled("Image format", format));

        var target = new OptionButton { CustomMinimumSize = new Vector2(220, 0) };
        target.AddItem("Windows x64");
        target.AddItem("macOS universal");
        target.AddItem("Linux x64");
        target.AddSeparator("Experimental");
        target.AddItem("Linux arm64");
        row.AddChild(Labelled("Platform", target));

        var locked = new OptionButton { CustomMinimumSize = new Vector2(200, 0), Disabled = true };
        locked.AddItem("Release");
        row.AddChild(Labelled("Configuration (locked)", locked));
        return row;
    }

    Control SliderSpecimen()
    {
        var col = Column(24);
        col.AddChild(Labelled("Opacity", SliderWithReadout(0, 100, 1, 80, "%", false)));
        col.AddChild(Labelled("Zoom (5 stops)", SliderWithReadout(25, 400, 25, 100, "%", false, ticks: 5)));
        col.AddChild(Labelled("Volume (muted)", SliderWithReadout(0, 100, 1, 40, "%", true)));
        return col;
    }

    Control SliderWithReadout(double min, double max, double step, double value, string unit, bool disabled, int ticks = 0)
    {
        var row = Row(16);
        var slider = new HSlider
        {
            MinValue = min, MaxValue = max, Step = step, Value = value, Editable = !disabled,
            CustomMinimumSize = new Vector2(360, 24), TickCount = ticks, TicksOnBorders = ticks > 0,
            FocusMode = disabled ? FocusModeEnum.None : FocusModeEnum.All,
        };
        var spin = new SpinBox { MinValue = min, MaxValue = max, Step = step, Value = value, Suffix = unit, Editable = !disabled, CustomMinimumSize = new Vector2(110, 0) };
        slider.ValueChanged += v => spin.SetValueNoSignal(v);
        spin.ValueChanged += v => slider.SetValueNoSignal(v);
        row.AddChild(slider);
        row.AddChild(spin);
        return row;
    }

    Control TypeSpecimen()
    {
        var col = Column(14);
        col.AddChild(new Label { Text = "Export presets", ThemeTypeVariation = "TitleLabel" });
        col.AddChild(new Label { Text = "Output and compression", ThemeTypeVariation = "HeadingLabel" });
        col.AddChild(Prose("Presets decide what leaves the project: which platforms are built, how images are compressed, and where the files land. Each preset is saved with the project and can be shared."));
        col.AddChild(new Label { Text = "Applies to every platform in this preset.", ThemeTypeVariation = "StrongLabel" });
        col.AddChild(new Label { Text = "Last exported 2026-09-14 10:32 · 214 files · 48.6 MB", ThemeTypeVariation = "CaptionLabel" });
        col.AddChild(new Label { Text = "Tabular figures  0123456789  1 111.11  8 888.88" });
        return col;
    }

    Control IconSpecimen()
    {
        var grid = new GridContainer { Columns = 8 };
        grid.AddThemeConstantOverride("h_separation", 0);
        grid.AddThemeConstantOverride("v_separation", 0);
        foreach (var name in Icons.Names)
        {
            var col = Column(8);
            col.Alignment = BoxContainer.AlignmentMode.Center;
            col.CustomMinimumSize = new Vector2(104, 76);
            col.AddChild(new Icon { IconName = name, CustomMinimumSize = new Vector2(24, 24) });
            col.AddChild(new Label { Text = name, ThemeTypeVariation = "CaptionLabel", HorizontalAlignment = HorizontalAlignment.Center });
            grid.AddChild(Cell(col, Colors.Transparent));
        }
        return grid;
    }

    // DataGrid: 10 000 synthetic project assets.
    static readonly string[] Kinds = ["Texture", "Scene", "Script", "Audio", "Font", "Material"];
    static readonly string[] Folders = ["textures/terrain", "textures/ui", "scenes/levels", "scripts/player", "audio/sfx", "fonts", "materials/props", "scenes/ui"];
    static readonly string[] Ext = [".png", ".tscn", ".cs", ".ogg", ".otf", ".tres"];

    Control GridSpecimen()
    {
        const int n = 10_000;
        var rng = new RandomNumberGenerator { Seed = 42 };
        var rows = new string[n][];
        var sizes = new double[n];
        var start = new DateTime(2026, 3, 1, 9, 0, 0);
        for (int i = 0; i < n; i++)
        {
            var k = rng.RandiRange(0, Kinds.Length - 1);
            var folder = Folders[rng.RandiRange(0, Folders.Length - 1)];
            sizes[i] = Math.Round(rng.RandfRange(0.4f, 4096f), 1);
            var modified = start.AddMinutes(rng.RandiRange(0, 300_000));
            rows[i] = [(i + 1).ToString("D5", Inv), $"res://{folder}/{Kinds[k].ToLowerInvariant()}_{i % 977:D3}{Ext[k]}", Kinds[k], sizes[i].ToString("N1", Inv), modified.ToString("yyyy-MM-dd HH:mm", Inv)];
        }
        var view = Enumerable.Range(0, n).ToArray();

        var grid = new DataGrid { CustomMinimumSize = new Vector2(0, 420), SizeFlagsHorizontal = SizeFlags.ExpandFill };
        grid.SetData(
        [
            new GridColumn { Title = "ID", Width = 76, Numeric = true },
            new GridColumn { Title = "Path", Width = 360, Expand = true },
            new GridColumn { Title = "Type", Width = 110 },
            new GridColumn { Title = "Size (KB)", Width = 110, Numeric = true },
            new GridColumn { Title = "Modified", Width = 150, Numeric = true },
        ], n, (r, c) => rows[view[r]][c]);

        string filter = "";
        int sortCol = -1;
        bool asc = true;
        void Apply()
        {
            IEnumerable<int> q = Enumerable.Range(0, n);
            if (filter != "") q = q.Where(i => rows[i][1].Contains(filter, StringComparison.OrdinalIgnoreCase));
            if (sortCol >= 0)
            {
                Func<int, IComparable> key = sortCol switch { 0 => i => i, 3 => i => sizes[i], _ => i => rows[i][sortCol] };
                q = asc ? q.OrderBy(key) : q.OrderByDescending(key);
            }
            view = q.ToArray();
            grid.RowCount = view.Length;
            grid.QueueRedraw();
        }
        grid.SortRequested += (c, a) => { sortCol = c; asc = a; Apply(); };

        var col = Column(12);
        var bar = Row(12);
        var search = new LineEdit { PlaceholderText = "Filter by path", CustomMinimumSize = new Vector2(280, 0), RightIcon = Icons.Get("search", _tokens.InkMuted), ClearButtonEnabled = true };
        search.TextChanged += s => { filter = s.Trim(); Apply(); };
        bar.AddChild(search);
        bar.AddChild(new Label { Text = "Synthetic data: 10,000 generated rows", ThemeTypeVariation = "CaptionLabel", SizeFlagsHorizontal = SizeFlags.ExpandFill, HorizontalAlignment = HorizontalAlignment.Right });
        col.AddChild(bar);
        col.AddChild(grid);
        return col;
    }

    Control TreeSpecimen()
    {
        var tree = new Tree { Columns = 3, ColumnTitlesVisible = true, SelectMode = Tree.SelectModeEnum.Row, CustomMinimumSize = new Vector2(0, 392), SizeFlagsHorizontal = SizeFlags.ExpandFill };
        tree.SetColumnTitle(0, "Name");
        tree.SetColumnTitle(1, "Type");
        tree.SetColumnTitle(2, "Size");
        tree.SetColumnTitleAlignment(0, HorizontalAlignment.Left);
        tree.SetColumnTitleAlignment(1, HorizontalAlignment.Left);
        tree.SetColumnTitleAlignment(2, HorizontalAlignment.Right);
        tree.SetColumnExpand(1, false);
        tree.SetColumnCustomMinimumWidth(1, 120);
        tree.SetColumnExpand(2, false);
        tree.SetColumnCustomMinimumWidth(2, 110);
        var folder = Icons.Get("folder", _tokens.InkMuted);
        var file = Icons.Get("file", _tokens.InkMuted);

        var root = tree.CreateItem();
        root.SetText(0, "res://");
        root.SetIcon(0, folder);
        TreeItem Add(TreeItem parent, string name, string type, string size, bool collapsed = false)
        {
            var it = tree.CreateItem(parent);
            it.SetText(0, name);
            it.SetIcon(0, type == "Folder" ? folder : file);
            it.SetText(1, type);
            it.SetText(2, size);
            it.SetTextAlignment(2, HorizontalAlignment.Right);
            it.Collapsed = collapsed;
            return it;
        }
        var scenes = Add(root, "scenes", "Folder", "");
        var levels = Add(scenes, "levels", "Folder", "");
        Add(levels, "level_001.tscn", "Scene", "18.4 KB");
        Add(levels, "level_002.tscn", "Scene", "21.0 KB");
        Add(levels, "level_003.tscn", "Scene", "19.7 KB");
        var ui = Add(scenes, "ui", "Folder", "");
        Add(ui, "main_menu.tscn", "Scene", "6.2 KB");
        Add(ui, "settings.tscn", "Scene", "9.8 KB");
        var scripts = Add(root, "scripts", "Folder", "", collapsed: true);
        Add(scripts, "Player.cs", "Script", "4.1 KB");
        var tex = Add(root, "textures", "Folder", "");
        Add(tex, "atlas_ui.png", "Texture", "812.0 KB");
        Add(tex, "grass_04.png", "Texture", "1,204.3 KB");
        Add(root, "project.godot", "Project", "1.3 KB");
        levels.GetChild(1).Select(0);
        return tree;
    }

    Control ListSpecimen()
    {
        var rng = new RandomNumberGenerator { Seed = 7 };
        var verbs = new[] { "Loaded scene res://scenes/levels/level_{0:D3}.tscn", "Imported texture res://textures/terrain/grass_{0:D2}.png", "Compiled shader terrain_{0}.gdshader", "Saved preset Release — desktop", "Cached 128 glyphs for D-DIN" };
        var t = new DateTime(2026, 9, 14, 10, 32, 0);
        var lines = new string[5000];
        for (int i = 0; i < lines.Length; i++)
        {
            t = t.AddMilliseconds(rng.RandiRange(4, 900));
            lines[i] = $"{t:HH:mm:ss.fff}    {string.Format(Inv, verbs[rng.RandiRange(0, verbs.Length - 1)], rng.RandiRange(1, 40))}";
        }
        var col = Column(12);
        col.AddChild(new Label { Text = "Synthetic log: 5,000 generated lines", ThemeTypeVariation = "CaptionLabel" });
        col.AddChild(new ListView { Items = lines, CustomMinimumSize = new Vector2(0, 320), SizeFlagsHorizontal = SizeFlags.ExpandFill, SelectedRow = 3, ShowFooter = true, RangeFormat = "Lines {0}–{1} of {2}" });
        return col;
    }

    Control InspectorSpecimen()
    {
        var spec = new DemoSpecimen();
        var row = Row(32);
        var inspector = new PropertyInspector { CustomMinimumSize = new Vector2(460, 0) };
        inspector.Inspect(spec);
        var frame = new PanelContainer();
        frame.AddThemeStyleboxOverride("panel", Rule(_tokens.Paper, _tokens.Rule, 1, 1, 1, 0));
        frame.AddChild(inspector);
        row.AddChild(frame);

        var readout = new Label { Text = Describe(spec), SizeFlagsHorizontal = SizeFlags.ExpandFill, AutowrapMode = TextServer.AutowrapMode.WordSmart };
        inspector.PropertyEdited += _ => readout.Text = Describe(spec);
        var side = Column(8);
        side.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        side.AddChild(new Label { Text = "Object state", ThemeTypeVariation = "StrongLabel" });
        side.AddChild(readout);
        row.AddChild(side);
        return row;
    }

    static string Describe(DemoSpecimen s) =>
        $"PresetName = \"{s.PresetName}\"\nImageFormat = {s.ImageFormat}\nQuality = {s.Quality}\nStripMetadata = {s.StripMetadata}\nUiScale = {s.UiScale.ToString(Inv)}\nColumnCount = {s.ColumnCount}\nAccentColor = #{s.AccentColor.ToHtml(false).ToUpperInvariant()}";

    // ---- sheet primitives ------------------------------------------------------------------

    Control RuledTable(string[] head, IEnumerable<string[]> body, float[] widths, bool[]? numeric = null, Action<int, int, Control>? decorate = null)
    {
        var grid = new GridContainer { Columns = head.Length };
        grid.AddThemeConstantOverride("h_separation", 0);
        grid.AddThemeConstantOverride("v_separation", 0);
        Control Make(string text, int col, bool header, int row)
        {
            var box = new HBoxContainer { CustomMinimumSize = new Vector2(widths[col], header ? 32 : 30) };
            if (widths[col] == 0) box.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            if (text != "")
                box.AddChild(new Label
                {
                    Text = text,
                    ThemeTypeVariation = header ? "StrongLabel" : "",
                    VerticalAlignment = VerticalAlignment.Center,
                    SizeFlagsHorizontal = SizeFlags.ExpandFill,
                    HorizontalAlignment = numeric?[col] == true ? HorizontalAlignment.Right : HorizontalAlignment.Left,
                });
            if (!header) decorate?.Invoke(row, col, box);
            box.Alignment = BoxContainer.AlignmentMode.Begin;
            return Cell(Margin(box, 12, 0, 12, 0), header ? _tokens.Band : Colors.Transparent, left: col > 0);
        }
        for (int c = 0; c < head.Length; c++) grid.AddChild(Make(head[c], c, true, -1));
        var r = 0;
        foreach (var line in body)
        {
            for (int c = 0; c < head.Length; c++) grid.AddChild(Make(line[c], c, false, r));
            r++;
        }
        var outer = new PanelContainer();
        outer.AddThemeStyleboxOverride("panel", Rule(_tokens.Paper, _tokens.Rule, 1, 1, 1, 0));
        outer.AddChild(grid);
        return outer;
    }

    /// <summary>A table cell: hairline at the bottom, and on the left when it is not the first column.</summary>
    PanelContainer Cell(Control content, Color fill, bool left = false)
    {
        var p = new PanelContainer { SizeFlagsHorizontal = content.SizeFlagsHorizontal };
        var box = Rule(fill, _tokens.Hairline, left ? 1 : 0, 0, 0, 1);
        p.AddThemeStyleboxOverride("panel", box);
        p.AddChild(content);
        return p;
    }

    static StyleBoxFlat Rule(Color fill, Color line, int left, int top, int right, int bottom) => new()
    {
        BgColor = fill, BorderColor = line,
        BorderWidthLeft = left, BorderWidthTop = top, BorderWidthRight = right, BorderWidthBottom = bottom,
    };

    Control Code(string code)
    {
        var p = new PanelContainer();
        p.AddThemeStyleboxOverride("panel", Rule(_tokens.Band, _tokens.Hairline, 1, 1, 1, 1));
        var mono = new SystemFont { FontNames = ["JetBrains Mono", "SF Mono", "Menlo", "Consolas", "DejaVu Sans Mono"] };
        var label = new Label { Text = code };
        label.AddThemeFontOverride("font", mono);
        label.AddThemeFontSizeOverride("font_size", 13);
        p.AddChild(Margin(label, 16, 12, 16, 12));
        return p;
    }

    VBoxContainer Labelled(string label, Control control)
    {
        var box = Column(6);
        box.AddChild(new Label { Text = label, ThemeTypeVariation = "FieldLabel" });
        box.AddChild(control);
        return box;
    }

    static Label Prose(string text) => new()
    {
        Text = text,
        AutowrapMode = TextServer.AutowrapMode.WordSmart,
        // One normative sentence on one line.
        CustomMinimumSize = new Vector2(820, 0),
        SizeFlagsHorizontal = SizeFlags.ShrinkBegin,
    };

    static MarginContainer Margin(Control c, int l, int t, int r, int b)
    {
        var m = new MarginContainer { SizeFlagsHorizontal = c.SizeFlagsHorizontal };
        m.AddThemeConstantOverride("margin_left", l);
        m.AddThemeConstantOverride("margin_top", t);
        m.AddThemeConstantOverride("margin_right", r);
        m.AddThemeConstantOverride("margin_bottom", b);
        m.AddChild(c);
        return m;
    }

    static Control Gap(int h) => new() { CustomMinimumSize = new Vector2(0, h) };

    static VBoxContainer Column(int sep)
    {
        var v = new VBoxContainer();
        v.AddThemeConstantOverride("separation", sep);
        return v;
    }

    static HBoxContainer Row(int sep)
    {
        var h = new HBoxContainer();
        h.AddThemeConstantOverride("separation", sep);
        return h;
    }

    static string Contrast(Color fg, Color bg)
    {
        static double L(Color c)
        {
            static double Ch(float v) => v <= 0.04045 ? v / 12.92 : Math.Pow((v + 0.055) / 1.055, 2.4);
            return 0.2126 * Ch(c.R) + 0.7152 * Ch(c.G) + 0.0722 * Ch(c.B);
        }
        var (a, b) = (L(fg), L(bg));
        var ratio = (Math.Max(a, b) + 0.05) / (Math.Min(a, b) + 0.05);
        return ratio.ToString("0.0", Inv) + " : 1";
    }

    /// <summary>Screenshot aid: focus the specimen's first control so the focus mark is captured.</summary>
    void FocusSpecimen()
    {
        static Control? First(Node n)
        {
            foreach (var c in n.GetChildren())
            {
                if (c is DataGrid or Button or LineEdit or Tree or Slider && ((Control)c).FocusMode == FocusModeEnum.All) return (Control)c;
                if (First(c) is { } hit) return hit;
            }
            return null;
        }
        var target = First(_sheet);
        target?.GrabFocus();
        if (target is DataGrid g) g.SelectedRow = 2;
    }

    /// <summary>Screenshot aid: drag the first column's header rule of the grid, mid-gesture.</summary>
    async System.Threading.Tasks.Task ResizeDemo()
    {
        for (int i = 0; i < 3; i++) await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        static DataGrid? Find(Node n) => n is DataGrid g ? g : n.GetChildren().Select(Find).FirstOrDefault(x => x is not null);
        if (Find(_sheet) is not { } grid) return;
        var edge = new Vector2(1 + grid.Columns[0].Width, 12);
        grid._GuiInput(new InputEventMouseButton { ButtonIndex = MouseButton.Left, Pressed = true, Position = edge });
        grid._GuiInput(new InputEventMouseMotion { Position = edge + new Vector2(58, 0) });
    }

    // ---- screenshot mode -------------------------------------------------------------------

    static Dictionary<string, string> ParseArgs() =>
        OS.GetCmdlineUserArgs().Select(a => a.TrimStart('-').Split('=', 2)).Where(p => p.Length == 2).ToDictionary(p => p[0], p => p[1]);

    async System.Threading.Tasks.Task Screenshot(string path)
    {
        // An occluded window may never draw on its own: process frames, then force one draw.
        OS.LowProcessorUsageMode = false;
        for (int i = 0; i < 8; i++) await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        RenderingServer.ForceDraw();
        var img = GetViewport().GetTexture().GetImage();
        img.SavePng(path);
        GD.Print($"shot {path} {img.GetWidth()}x{img.GetHeight()}");
        GetTree().Quit();
    }
}
