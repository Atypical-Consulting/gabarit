---
name: Gabarit
description: Desktop UI components for Godot 4 .NET, specified like a DIN/ISO standards sheet.
colors:
  ground: "#f2f2f0"
  paper: "#ffffff"
  band: "#e9e9e6"
  ink: "#1b1c1e"
  ink-muted: "#55585c"
  rule: "#7f8287"
  hairline: "#d6d6d2"
  action: "#0b5cad"
  action-deep: "#094e93"
  on-action: "#ffffff"
  action-tint: "#dce7f4"
  danger: "#b3261e"
  slate-ground: "#131416"
  slate-paper: "#1c1e21"
  slate-band: "#25282c"
  slate-ink: "#ecebe6"
  slate-ink-muted: "#a5a8ad"
  slate-rule: "#7c8086"
  slate-hairline: "#34373c"
  slate-action: "#7ab0f0"
  slate-action-deep: "#6896cc"
  slate-on-action: "#0d1520"
  slate-action-tint: "#22354d"
  slate-danger: "#f08a80"
typography:
  title:
    fontFamily: "Gabarit Figures, D-DIN"
    fontSize: "30px"
    fontWeight: 700
  headline:
    fontFamily: "Gabarit Figures, D-DIN"
    fontSize: "18px"
    fontWeight: 700
  body:
    fontFamily: "Gabarit Figures, D-DIN"
    fontSize: "14px"
    fontWeight: 400
  body-strong:
    fontFamily: "Gabarit Figures, D-DIN"
    fontSize: "14px"
    fontWeight: 700
  label:
    fontFamily: "Gabarit Figures, D-DIN"
    fontSize: "12px"
    fontWeight: 700
  caption:
    fontFamily: "Gabarit Figures, D-DIN"
    fontSize: "12px"
    fontWeight: 400
  code:
    fontFamily: "JetBrains Mono, SF Mono, Menlo, Consolas, DejaVu Sans Mono"
    fontSize: "13px"
    fontWeight: 400
rounded:
  sheet: "2px"
  carriage: "1px"
spacing:
  unit: "4px"
  sm: "8px"
  md: "12px"
  lg: "16px"
  row: "26px"
  control: "30px"
components:
  button:
    backgroundColor: "{colors.paper}"
    textColor: "{colors.ink}"
    rounded: "{rounded.sheet}"
    padding: "4px 12px"
    height: "30px"
  button-hover:
    backgroundColor: "{colors.band}"
    textColor: "{colors.ink}"
  button-pressed:
    backgroundColor: "{colors.ink}"
    textColor: "{colors.paper}"
  button-primary:
    backgroundColor: "{colors.action}"
    textColor: "{colors.on-action}"
    typography: "{typography.body-strong}"
    rounded: "{rounded.sheet}"
    padding: "4px 12px"
    height: "30px"
  button-primary-hover:
    backgroundColor: "{colors.action-deep}"
    textColor: "{colors.on-action}"
  button-primary-pressed:
    backgroundColor: "{colors.ink}"
    textColor: "{colors.paper}"
  button-flat:
    backgroundColor: "transparent"
    textColor: "{colors.ink}"
    rounded: "{rounded.sheet}"
    padding: "4px 8px"
  button-flat-hover:
    backgroundColor: "{colors.band}"
  text-field:
    backgroundColor: "{colors.paper}"
    textColor: "{colors.ink}"
    rounded: "{rounded.sheet}"
    padding: "4px 8px"
    height: "30px"
  text-field-read-only:
    backgroundColor: "{colors.ground}"
    textColor: "{colors.ink-muted}"
  data-grid-header:
    backgroundColor: "{colors.band}"
    textColor: "{colors.ink}"
    typography: "{typography.body-strong}"
    height: "30px"
    padding: "0 8px"
  data-grid-row:
    backgroundColor: "{colors.paper}"
    textColor: "{colors.ink}"
    typography: "{typography.body}"
    height: "26px"
    padding: "0 8px"
  data-grid-row-selected:
    backgroundColor: "{colors.action-tint}"
    textColor: "{colors.ink}"
  data-grid-footer:
    backgroundColor: "{colors.band}"
    textColor: "{colors.ink-muted}"
    typography: "{typography.caption}"
    height: "26px"
  tooltip:
    backgroundColor: "{colors.ink}"
    textColor: "{colors.paper}"
    typography: "{typography.caption}"
    rounded: "{rounded.sheet}"
    padding: "6px 8px"
---

# Design System: Gabarit

## Overview

**Creative North Star: "The Standards Sheet"**

Every component is specified the way a DIN/ISO standard specifies a part: a numbered clause, a one-line normative sentence, a ruled table of states, a title block. The library ships this world as a Godot `Tokens` resource turned into a `Theme` by `ThemeBuilder`; the Gallery is the sheet that proves it. One ink, graphite rules at 1px, white paper on a warm-grey ground, and a single action blue that appears only where something can be acted on or has been selected.

Density is desktop density: 30px controls, 26px rows, a 4px unit. Form is drawn, not modelled: 1px frames, 2px corners at most, no gradients, flat fills. States are carried by marks wherever the component allows it (filled ink for pressed, a dashed rule for unavailable, a 2px ink frame outside the control for focus), and cells never change size with state. Where the build uses a fill for a state (hover band, selection tint) it is a quiet, same-size fill, never a glow.

The dark sheet, Slate, is the same sheet inverted: same token names, values swapped, same contrast rules. Everything visual flows from `Tokens`; a consumer re-skins by saving a `.tres` and overriding values.

**Key Characteristics:**
- One ink, one action colour, graphite rules; two sheets (Paper, Slate) sharing token names.
- 1px rules everywhere; radius 2px maximum, 1px on carriages.
- States as marks: pressed = filled ink, disabled/read-only = dashed rule, focus = 2px ink frame 2px outside.
- D-DIN grotesk with a generated tabular-figures face in front of it.
- Fully ruled tables with numbered guide words in the footer.
- Authored 16px line icons, one 1.5px square-capped stroke.

## Colors

A paper-and-ink palette: warm neutral greys, near-black ink, one engineering blue, one red reserved for errors.

### Primary
- **Drawing-Office Blue** (`action`; Slate `slate-action`): primary button fill, switch "on" slot, slider fill up to the reading and highlighted carriage, SpinBox hover arrow, the DataGrid resize dimension line. Hover deepens it 15% (`action-deep`).
- **Selection Wash** (`action-tint`): selected rows in DataGrid, Tree, ItemList and PopupMenu hover; text selection in fields. Ink text stays on top.

### Neutral
- **Ink** (`ink`): all text, pressed fills, focus frames, checked checkbox fill, the tooltip surface.
- **Muted Ink** (`ink-muted`): captions, placeholders, disabled and read-only text, footer guide words, clause numbers.
- **Graphite Rule** (`rule`): control outlines, frames of sheets and grids. Kept at 3:1 or better against Paper (WCAG 1.4.11).
- **Hairline** (`hairline`): internal table rules, separators, slider track, tree guides. Decorative; no contrast requirement.
- **Paper** (`paper`): controls, tables, sheets.
- **Ground** (`ground`): application background behind sheets; read-only field fill; DataGrid row hover.
- **Band** (`band`): table header and footer bands, button hover, code blocks, title-block surroundings.

### Tertiary
- **Error Red** (`danger`): error field rule and error caption only.

### Named Rules
**The One Signal Rule.** The action blue marks what can be acted on or what is selected. It is never decoration, never a heading colour, never a border on a static surface. One primary button per view.

**The Same Names Rule.** Slate is not a second palette: it is the same token names with inverted values. New tokens are added to both presets or to neither.

**The Outline Contrast Rule.** `rule` must keep 3:1 against `paper` on every preset; `hairline` is allowed to be faint because it never bounds an interactive control alone.

## Typography

**Body Font:** D-DIN (Regular, Bold; OFL), loaded behind Gabarit Figures
**Figures Font:** Gabarit Figures (digits-only, one shared advance, generated by `tools/make_figures.py`), first in the fallback chain so every number is tabular
**Code Font:** system monospace (JetBrains Mono, SF Mono, Menlo, Consolas, DejaVu Sans Mono), Gallery usage blocks only

**Character:** an engineering-drawing grotesk; narrow, upright, and dry. Digits line up in columns because a figures-only face sits in front of D-DIN.

### Hierarchy
- **Title** (bold, 30px; `TitleLabel`): clause number and title on a sheet; the number is set in muted ink, the title in ink.
- **Headline** (bold, 18px; `HeadingLabel`): sheet sections ("States", "Usage"), app masthead.
- **Body** (regular, 14px): default for every control, cell and paragraph.
- **Body Strong** (bold, 14px; `StrongLabel`): table header cells, primary button label, emphasised values.
- **Label** (bold, 12px; `FieldLabel`): the label above a field or a specimen group.
- **Caption** (regular, 12px, muted ink; `CaptionLabel`): help text, title-block keys, guide words, tooltips. `ErrorLabel` is the same size in Error Red.

### Named Rules
**The Tabular Figures Rule.** Numbers are always set through the Tokens fonts, never a bare D-DIN instance: D-DIN's own digits are proportional and columns would wander.

**The Two Weights Rule.** Regular and Bold only, at 12 / 14 / 18 / 30px. Hierarchy comes from size, weight and muted ink, not from new faces.

## Layout

Everything sits on a 4px unit (`Tokens.Unit`): control padding is 3 units horizontal, 1 unit vertical; flat buttons 2 units; DataGrid cell padding 2 units; tree indentation 5 units; property-inspector key column 40 units. Controls are 30px tall, grid rows 26px, the grid header one unit taller (30px), the footer one row (26px).

The Gallery sheet is a fixed spec layout: a 264px ruled index on the left (clause list, current clause framed in ink), a scrolling sheet with 48px side margins, and a title block pinned bottom-right (Component, Class, Version, Page, Sheet). On the sheet: clause number + title, one normative sentence (max 820px), the live specimen in a 1px ruled frame with 28px inset, then "States" and "Usage" with 40px between sections and 12px under each heading.

### Named Rules
**The Fixed Slot Rule.** A cell never changes size with state. Error text replaces help text in the same line; focus draws outside the control; borders do not thicken except the field focus rule, which stays inside the same box.

## Elevation & Depth

Flat by default. Depth is drawn with rules and tonal bands (ground behind paper, band on headers), not shadows. The single shadow in the system belongs to popups (PopupMenu, the Select list), which genuinely float over the sheet.

### Shadow Vocabulary
- **Popup lift** (black at 18% opacity, 10px size, 0 3px offset): PopupMenu panel only.

### Named Rules
**The Drawn Depth Rule.** Sheets, panels, tables and controls carry no shadow. If something needs separating, rule it or change its band.

## Shapes

Rectangles. Every control box and checkbox uses a 2px corner (`Tokens.Radius`), focus frames follow it; the switch carriage, slider carriage and scrollbar grabbers use 1px. Strokes are 1px and non-antialiased so they land on the pixel grid; the only 2px strokes are focus and the focused field rule. The dashed rule (3px dash, 3px gap; 2px/2px inside checkbox and switch marks) is the library's mark for "not available".

## Components

### Buttons
Drawn, not lit.
- **Shape:** 2px corners, 1px rule, 30px tall.
- **Default:** paper fill, graphite rule, ink text. Hover: band fill, ink rule.
- **Primary (`PrimaryButton`):** action fill, on-action bold text; hover deepens 15%.
- **Pressed (all variants):** filled ink, paper text.
- **Disabled:** dashed graphite rule, muted text.
- **Flat (`FlatButton`):** no frame until hovered (band fill); for toolbars and icon-only actions.
- **Focus:** 2px ink frame set 2px outside.

### Checkbox and Switch
The mark carries the state; the label never inverts.
- **Checkbox:** 16px square, 2px corner. Off: paper with graphite rule. On: ink fill with paper check. Disabled: dashed frame, hairline fill.
- **Switch:** 32x16 ruled slot with a square 12x10 carriage. Off: open slot, ink carriage left. On: action-filled slot, on-action carriage right. Disabled: dashed slot.

### Inputs / Fields
- **Style:** paper fill, 1px graphite rule, 2px corner, 30px tall, 8px horizontal padding.
- **Focus:** the rule turns ink and 2px; caret is ink, 2px wide.
- **Error (`ErrorLineEdit`):** Error Red rule (2px on focus); message in the caption slot below.
- **Read-only:** dashed rule on ground, muted text, skipped by Tab.
- **Field:** label (bold 12px) above, input, then one caption line that holds help or error, never both.

### Select, Slider, Scrollbars
- **Select:** a field-shaped button with an ink chevron; the popup is the one elevated surface, items hover in Selection Wash with an ink check for the chosen one.
- **Slider:** a hairline track, ink fill up to the reading, a vertical 6x20 carriage like an index mark on a scale; graduations are short graphite hairlines. Highlight turns the fill and carriage action blue.
- **Scrollbars:** thin graphite carriages with no trough and no arrow buttons.

### DataGrid / ListView / TreeView
The signature: a fully ruled, virtualized table.
- 1px graphite frame, hairline row and column rules, band header in bold, 26px rows; numeric columns right-aligned in tabular figures.
- Row hover: ground fill. Selected: Selection Wash. Keyboard cursor: 1px ink outline.
- **Guide words:** the footer band names the fully visible range ("Rows 1–14 of 10,000") in muted caption type and recomputes while scrolling; selection count sits right.
- **Dimension line:** while a column is dragged, an action-blue dimension line with end ticks and a "134 px" reading is drawn in the header.
- ListView is the same grid without header or footer; Tree and ItemList share the ruled frame, hairline 90° guides and tinted selection.

### Tooltip
The one inverted surface: ink panel, paper caption text, 2px corner.

### Icons
Sixteen authored 16px SVG line icons (check, chevrons, sort, search, plus, minus, close, filter, folder, file, download, refresh, settings), one 1.5px square-capped mitered stroke, rendered as DPI-aware textures in the requested token colour.

### Gallery sheet furniture
- **Index:** a Tree ruled like a table of contents; the current clause is framed in 1px ink and set bold, not tinted.
- **Ruled table:** band header row in bold, hairline cell rules, 30px rows, graphite outer frame.
- **Title block:** a ruled strip of key/value cells (caption key over value) pinned bottom-right.
- **Usage block:** band fill, hairline frame, 13px system monospace.

## Do's and Don'ts

### Do:
- **Do** take every colour, size and metric from `Tokens`; add a token to both Paper and Slate rather than hard-coding a value.
- **Do** express states as marks first: filled ink for pressed, dashed rule for unavailable, 2px ink frame 2px outside for focus.
- **Do** keep every spacing a multiple of the 4px unit and every control at 30px, every row at 26px.
- **Do** route numbers through the Tokens fonts so figures stay tabular, and right-align numeric columns.
- **Do** rule tables fully: graphite frame, hairline internal rules, band header.
- **Do** draw new icons in the same 16px, 1.5px square-capped stroke.

### Don't:
- **Don't** use the action blue for anything that cannot be acted on or is not selected.
- **Don't** round corners beyond 2px, or add gradients, glows or coloured focus halos.
- **Don't** add shadows to sheets, panels, tables or controls; the popup lift is the only one.
- **Don't** let a state change a cell's size: no growing borders on rest controls, no error line that pushes the layout.
- **Don't** introduce a third weight or a second text family beyond the code block's monospace.
