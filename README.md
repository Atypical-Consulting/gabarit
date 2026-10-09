# Gabarit

Desktop UI components for Godot 4 (.NET / C#): a token-driven theme for the native controls,
plus the data controls Godot does not ship — a virtualized `DataGrid`, `ListView`,
`PropertyInspector` and a labelled `Field`.

Status: 0.1.0, preview. MIT licensed (see `LICENSE`); fonts under the SIL OFL 1.1.

## Install

1. Copy `addons/gabarit/` into your project.
2. Build the C# solution once (`dotnet build`).
3. Theme your root control:

```csharp
using Gabarit;

public override void _Ready()
{
    Theme = ThemeBuilder.Build(Tokens.CreatePaper()); // or Tokens.CreateSlate()
}
```

To re-skin, change any value on a `Tokens` resource (or save one as `.tres` and edit it in the
inspector) and rebuild the theme.

## What is inside

| Clause | Component | Class |
|---|---|---|
| 1.1 | Tokens | `Tokens`, `ThemeBuilder` |
| 1.2 | Button | native `Button`, variations `PrimaryButton`, `FlatButton` |
| 1.3 | Text field | `Field` |
| 1.4 | Checkbox | native `CheckBox` |
| 1.5 | Switch | native `CheckButton` |
| 1.6 | Select | native `OptionButton` |
| 1.7 | Slider | native `HSlider` |
| 1.8 | Type | `Label` variations: `TitleLabel`, `HeadingLabel`, `StrongLabel`, `CaptionLabel`, `FieldLabel`, `ErrorLabel` |
| 1.9 | Icon | `Icon`, `Icons` |
| 2.1 | DataGrid | `DataGrid` |
| 2.2 | TreeView | native `Tree` |
| 2.3 | ListView | `ListView` |
| 2.4 | PropertyInspector | `PropertyInspector` |

## Gallery

This repository is itself a Godot project: open it in Godot 4.7 .NET and run it to browse every
component and its states. Screenshot mode: `godot-mono --path . -- shot=/tmp/out.png clause=2.1 sheet=slate scale=1`.

## Credits

Typeface: D-DIN by Datto, SIL Open Font License 1.1 (`addons/gabarit/fonts/OFL.txt`).
