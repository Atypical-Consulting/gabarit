# Product

<!-- impeccable:product-schema 1 -->

## Platform

desktop

<!-- Not web, iOS or Android: a Godot 4 addon whose UI renders inside the Godot engine
     (Control nodes + Theme resources) on Windows, macOS and Linux desktop apps. -->

## Stack

Godot 4.x .NET (C#). Developed against Godot 4.7.2 mono and .NET SDK 10.0.4xx on this machine.
Distributed as an addon under `addons/`. C# means the library does not serve GDScript-only
projects or Godot's web export; that trade was accepted.

## Users

Developers building **desktop applications** (tools, editors, utilities) with Godot rather than
games: people who chose Godot as an app framework and need interface building blocks the default
editor-style theme does not provide. They work in C#, inside the Godot editor, composing scenes.

## Product Purpose

A public, open-source library of UI components (Control-based) for building desktop interfaces
in Godot. Success: a developer can assemble a credible, accessible desktop app UI from the
library's components and re-skin it to their own brand through tokens, without writing custom
drawing code.

## Positioning

Godot ships game-oriented controls and an editor-flavoured default theme. This library targets
the desktop-application job specifically: data-heavy controls and a themable token system,
written in C#.

## Operating Context

- Consumed as a Godot addon, components dropped into scenes or instantiated from C#.
- Shown through a **Gallery** Godot project, built with the library itself, presenting every
  component and its states (Storybook-style).
- Released publicly (GitHub; Godot Asset Library is the natural channel, not yet decided).

## Capabilities and Constraints

v1 scope:
- **Foundations:** design tokens + Godot Theme; Button, TextField, Checkbox, Switch, Select,
  Slider, Label, Icon.
- **Data:** DataGrid/Table, TreeView, virtualized ListView, PropertyInspector.

Out of v1 (not rejected, just later): app shell (MenuBar, Toolbar, StatusBar, docking panels,
tabs) and feedback (Dialog, Toast, Tooltip, ContextMenu, CommandPalette).

Visual stance: **neutral and themable** — no single game genre; each consuming app re-skins.

Name: **Gabarit** (decided 2026-10-10; free on NuGet and the Godot Asset Library).

License: MIT (decided 2026-10-10). Open decisions: Asset
Library publication, minimum supported Godot 4.x version.

## Evidence on Hand

None yet: empty repository. No users, screenshots, benchmarks or testimonials exist; do not
fabricate any.

## Product Principles

1. Desktop first: density, keyboard flow and data handling over game-HUD flourish.
2. Themable by tokens: every visual decision flows from a token a consumer can override.
3. Native to Godot: Control nodes, Theme resources, signals and the editor workflow — no parallel UI runtime.
4. The Gallery is the proof: a component isn't done until it is shown there in all its states.

## Accessibility & Inclusion

Full keyboard navigation with visible focus, WCAG 2.1 AA contrast on default tokens, respect for
UI scale. Screen-reader exposure through Godot's AccessKit integration where the engine supports it.
