# Dialogue Game Engine

A lightweight, data-driven engine for branching narrative games — stories told through scenes, choices, attributes, and flags. Write your story as plain JSON files; the engine handles all the state and logic.

## Quick Start

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download)

```sh
# Build everything
dotnet build

# Play the included sample story
dotnet run --project DialogueGameEngine.Cli -- --story DialogueGameEngine.Cli/sample-story

# Play the tutorial story (great starting point)
dotnet run --project DialogueGameEngine.Cli -- --story docs/examples/tutorial-story

# Validate a story before playing it
dotnet run --project DialogueGameEngine.Cli -- validate --story docs/examples/tutorial-story
```

## Playing a Story

```sh
dotnet run --project DialogueGameEngine.Cli -- --story <dir> [--save <file>] [--start <scene-id>]
```

| Argument | Default | Description |
|----------|---------|-------------|
| `--story` | *(required)* | Directory containing `.json` scene files (scanned recursively) |
| `--save` | `save.json` next to `--story` | Path to the save file |
| `--start` | first scene alphabetically | Scene ID to begin from when no save file exists |

### Controls

| Key | Action |
|-----|--------|
| `1`–`9` | Select a choice |
| `s` | Save progress manually |
| `d` | Print a debug inspect snapshot |
| `q` | Save and quit |

Progress is **auto-saved** after every choice. To restart, delete `save.json`.

When a new session starts, the engine runs the initial scene's `onEnterEffects` once before play begins. Saved games remember that this happened, so resuming a save does not replay initial enter effects.

## Building a Story

### Using the Interactive Editor

The CLI includes a full story editor — create and modify scenes, choices, conditions, and effects through guided menus with no JSON knowledge required:

```sh
dotnet run --project DialogueGameEngine.Cli -- edit --story <dir>
```

If the directory does not exist it will be created. The editor lets you:

- Create and delete scenes
- Edit scene title, description, and participants
- Add and edit choices with conditions and effects
- Manage attribute ambiance, modifiers, and lifecycle effects
- Save individual scenes to disk

### Story Directory Structure

Scenes are scanned recursively, so you can organise them however suits your story:

```
my-story/
├── 0001-opening.json       # 4-digit prefix keeps files in play order in your editor
├── 0002-confrontation.json
├── 0003-resolution.json
└── _initial-state.json     # optional: seed starting attribute values and opening scene
```

Or group scenes into subdirectories by act, chapter, or area:

```
my-story/
├── act1/
│   ├── 0001-opening.json
│   └── 0002-confrontation.json
├── act2/
│   ├── 0001-midpoint.json
│   └── 0002-resolution.json
└── _initial-state.json
```

- Each `.json` file whose **filename does not start with `_`** is one scene definition. The engine scans subdirectories recursively; directory names are not restricted.
- The **4-digit prefix** (`0001-`, `0002-`, …) is a convention for human readability only — the scene's `"id"` field inside the JSON is its identity. Subdirectories and prefixes have no effect on the engine.
- `_initial-state.json` sets starting attribute values and the opening scene. Without it, the engine starts at the first scene alphabetically with all attributes at zero.

### Analysing a Story

Use `validate` when you need a pass/fail correctness check:

```sh
dotnet run --project DialogueGameEngine.Cli -- validate --story <dir>
dotnet run --project DialogueGameEngine.Cli -- validate --story <dir> --warnings-as-errors
dotnet run --project DialogueGameEngine.Cli -- validate --story <dir> --format json
```

Validation exits with code `0` when there are no errors. Warnings do not fail by default; `--warnings-as-errors` makes them fail. The command checks JSON loading, required fields, duplicate scene and choice IDs, graph references, initial state, attribute addresses, conditions, effects, and lifecycle hooks.

Example validation error:

```text
ERROR STORY005 Scene 'intro', choice 'lie': unknown scene reference 'escape_route'.
```

Print graph statistics — scene count, choice count, reachable endings, unique paths, orphan scenes, and broken references:

```sh
dotnet run --project DialogueGameEngine.Cli -- stats --story <dir> [--start <scene-id>]
```

The optional `--start` overrides which scene is used as the root for reachability and path analysis. If omitted, the starting scene is read from `_initial-state.json` (or the first scene alphabetically if that file is absent).

`stats` is an analysis summary. `validate` is the correctness gate and returns failing exit codes for errors.

### Inspecting Runtime State

Print a full debug snapshot of the current save file: current scene, flags, attributes, active scene modifiers, and each current-scene choice with its condition, effects, availability, and destination.

```sh
dotnet run --project DialogueGameEngine.Cli -- inspect --story <dir> [--save <file>] [--start <scene-id>]
```

If a save file exists, `inspect` reads it. Otherwise it uses `--start`, `_initial-state.json`, or the first scene alphabetically in the same order as play mode. During play, press `d` to print the same snapshot without leaving the story.

### Scene File at a Glance

```json
{
  "id": "my_scene",
  "title": "The Kitchen",
  "description": "Mira stands by the window, arms crossed.",
  "participants": ["Player", "Mira"],
  "choices": [
    {
      "id": "apologise",
      "text": "\"I'm sorry. I should have told you.\"",
      "effects": [
        {
          "$type": "ChangeAttributeEffect",
          "target": { "scope": "Relation", "from": "Mira", "to": "Player", "attribute": "Trust" },
          "delta": 5
        },
        { "$type": "SetFlagEffect", "flag": "Apologised" }
      ],
      "nextScene": "mira_responds"
    },
    {
      "id": "deny",
      "text": "\"I don't know what you're talking about.\"",
      "effects": [
        {
          "$type": "ChangeAttributeEffect",
          "target": { "scope": "Relation", "from": "Mira", "to": "Player", "attribute": "Trust" },
          "delta": -8
        }
      ],
      "nextScene": "mira_responds"
    }
  ]
}
```

The story ends when the current scene has no available choices.

### Minimal Valid Story

```text
my-story/
├── _initial-state.json
└── start.json
```

`_initial-state.json`:

```json
{
  "currentScene": "start",
  "attributes": [],
  "flags": []
}
```

`start.json`:

```json
{
  "id": "start",
  "title": "A Beginning",
  "description": "The story starts here.",
  "choices": []
}
```

Scene files are loaded strictly by default for `play`, `stats`, `inspect`, and `validate`. Invalid JSON, unknown condition/effect types, missing required fields, and duplicate scene IDs fail instead of being skipped.

### Lifecycle Effects

Scene lifecycle effects are executed by the Core engine, not by the CLI:

1. `DialogueEngine.Start(state)` applies the initial scene's `onEnterEffects` once.
2. Selecting an available choice applies the current scene's `onExitEffects`.
3. The selected choice's `effects` run.
4. The engine moves to the choice's `nextScene` when one is set.
5. The target scene's `onEnterEffects` run.

Reading or displaying the current scene does not rerun `onEnterEffects`. Returning to a scene later runs that scene's `onEnterEffects` again.

### Attribute System

Attributes are integer values clamped to **–100 → +100** at runtime. Validation warns about out-of-range values because they will be clamped. Attributes live on one of four scopes:

| Scope | Example address | Meaning |
|-------|----------------|---------|
| `Character` | `Player.Openness` | A trait of a single character |
| `Relation` | `Mira→Player.Trust` | How one character feels about another (directional) |
| `Scene` | `kitchen.Tension` | Atmospheric quality of a specific scene |
| `World` | `World.DaysPassed` | Global counter shared across all scenes |

**Effective vs. base value:** Modifiers temporarily adjust an attribute's *effective* value without touching the stored base value. Conditions compare against the effective value by default.

### Conditions

Gate choices on game state. Use the editor, or write JSON directly:

| `$type` | What it checks |
|---------|---------------|
| `AttributeCondition` | attribute `operator` threshold (e.g. `Openness > 5`) |
| `FlagCondition` | whether a flag is set or not |
| `AllCondition` | logical AND of a list of conditions |
| `AnyCondition` | logical OR of a list of conditions |
| `NotCondition` | logical NOT of one condition |

### Effects

Applied when a choice is selected or a scene is entered/exited:

| `$type` | What it does |
|---------|-------------|
| `ChangeAttributeEffect` | adds `delta` to an attribute (clamped) |
| `SetAttributeEffect` | sets an attribute to an exact `value` |
| `SetFlagEffect` | marks a flag as happened |
| `ClearFlagEffect` | removes a flag |
| `MoveToSceneEffect` | transitions to a scene (prefer `nextScene` on the choice) |
| `ConditionalEffect` | branches between `then`/`else` effect lists at runtime |

## Format Reference

See [docs/story-format.md](docs/story-format.md) for the complete JSON format reference with examples for every field, condition type, and effect type.

## Examples

[docs/examples/tutorial-story/](docs/examples/tutorial-story/) is a fully-worked 3-scene story illustrating:

- Always-visible choices with attribute and flag effects
- An `AttributeCondition` that gates a choice based on accumulated state
- A scene modifier that temporarily shifts effective values
- A `ConditionalEffect` in `onEnterEffects` that branches on a flag

To play it:

```sh
dotnet run --project DialogueGameEngine.Cli -- --story docs/examples/tutorial-story
```

The [sample-story/](DialogueGameEngine.Cli/sample-story/) in the CLI project is a more complete 6-scene branching narrative demonstrating the full range of system features.
