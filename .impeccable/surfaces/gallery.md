---
version: 1
slug: "gallery"
primary_target: "gallery"
related_targets: []
---

# Surface: Gallery (Godot app) + component theme

Scope: the addon's components and the Gallery project that shows them. Mode: Operate.
Audience: C# developers building desktop tools with Godot. Task: find a component, see every
state, read its properties, judge it on both themes. Constraints: Godot 4.7 .NET, native Control
nodes + Theme, keyboard complete, WCAG AA on default tokens.

## Direction contract

THESIS: Every component is specified like a DIN/ISO standard sheet — numbered clause, ruled table,
title block, one ink. Refuses the category default (slate dark kit, 6px radii, blue glow).

OWN-WORLD: White paper #ffffff / sheet #f2f2f0, ink #1b1c1e, graphite rules #8a8d91 at 1px,
one action blue #0b5cad, used only where something can be acted on. DIN-style grotesk (D-DIN),
tabular figures. Radius 2px max. States are marks, not tints: pressed = filled ink, disabled =
struck through/dotted rule, focus = 2px ink frame offset 2px. Cells never change size with state.
Dark variant "ardoise" is the same sheet inverted, same token names.

STORY: The developer opens the Gallery, reads the clause list like a standard's table of contents,
opens a clause, uses the live specimen, reads its property/state table, flips paper/ardoise, and
trusts it enough to drop it into their own app.

FIRST VIEWPORT: Left: clause index (1 Fondations · 1.1 Button … 2 Données · 2.1 DataGrid …),
ruled, current clause in ink. Main sheet: clause number + title large at top left, one-line
normative sentence, the live specimen inside a 1px ruled frame filling the upper half; below, a
fully ruled table "States", then "Usage" code; title block pinned bottom-right (component, class,
version, sheet toggle Paper/Slate). Opens on 2.1 DataGrid with 10,000 synthetic rows; the grid's
footer shows the visible range "Rows 1–14 of 10,000". Copy is English (public Godot addon). Signature interaction: the range guide words recompute while scrolling, and
column widths are dimensioned like a drawing (width shown in px while resizing).

FORM: Feuille de norme (DIN/ISO standards sheet), own grounded list position 7 of 7, seed key 588f80b2.

FINISH: unreviewed and undocumented is unfinished; this build ends with the finish review, the verdict, DESIGN.md, and every shipping raster carrying its provenance

Raises: lexicon → visible-range guide words (grid footer); ticket wallet → states in fixed fields; transit →
90° guides on the 4px grid; Saville → nothing labeled twice; video feed → recycled rows, no flash.

Unresolved: library name and license (PRODUCT.md open decisions).
