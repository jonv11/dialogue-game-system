# Story Format Reference

Stories are directories of JSON files — one file per scene. The engine reads every `.json` file in the story directory and its subdirectories whose **filename** does not start with `_`, plus an optional `_initial-state.json` for seeding starting values.

---

## File Naming and Organisation

Scene files can be named freely, but the following conventions make large stories easier to navigate:

**4-digit ordering prefix** — prefix each filename with a zero-padded number so files appear in play order in your editor or file browser:

```text
0001-opening.json
0002-confrontation.json
0003-resolution.json
```

**Subdirectory grouping** — organise scenes into subdirectories by act, chapter, or location. The engine scans recursively:

```text
act1/
  0001-opening.json
  0002-confrontation.json
act2/
  0001-midpoint.json
  0002-resolution.json
_initial-state.json
```

The prefix and directory path are purely for human organisation. The scene's `"id"` field inside the JSON is the only identity the engine uses. The `_` exclusion applies to the **filename only** — a directory named `_archive/` does not cause scenes inside it to be skipped; only a file named `_something.json` is excluded.

---

## Scene File

Each scene file describes one unit of narrative. The file name is arbitrary; the `"id"` field is the scene's identity.

```json
{
  "id": "scene_id",
  "title": "Short Title",
  "description": "Longer opening text shown to the player.",
  "participants": ["Player", "Mira"],
  "ambiance": [ ... ],
  "modifiers": [ ... ],
  "choices": [ ... ],
  "onEnterEffects": [ ... ],
  "onExitEffects": [ ... ]
}
```

| Field | Required | Type | Description |
|-------|----------|------|-------------|
| `id` | yes | string | Unique identifier — `snake_case` by convention |
| `title` | yes | string | Short heading displayed at the top of the scene |
| `description` | no | string | Opening narration or dialogue |
| `participants` | no | string[] | Characters present (informational only) |
| `ambiance` | no | AttributeAssignment[] | Static attribute values describing the scene's atmosphere |
| `modifiers` | no | ModifierDefinition[] | Temporary adjustments to effective attribute values |
| `choices` | no | ChoiceDefinition[] | Player-facing options; empty = story ends here |
| `onEnterEffects` | no | Effect[] | Applied when the player enters this scene |
| `onExitEffects` | no | Effect[] | Applied when the player leaves this scene |

---

## Choice

```json
{
  "id": "confess",
  "text": "\"Yes. I broke it.\"",
  "condition": { ... },
  "effects": [ ... ],
  "nextScene": "mira_reacts"
}
```

| Field | Required | Type | Description |
|-------|----------|------|-------------|
| `id` | yes | string | Unique within this scene — describes the action (`"confess"`, `"lie"`) |
| `text` | yes | string | The text shown to the player |
| `condition` | no | Condition | If absent, the choice is always available |
| `effects` | no | Effect[] | Applied in order when the choice is selected |
| `nextScene` | no | string | Scene ID to move to after effects; omit to stay in the current scene |

---

## Attribute Address

Every condition, effect, and modifier targets an attribute via an address. The `"scope"` field determines which other fields are required.

### Character scope

An attribute belonging to one character.

```json
{ "scope": "Character", "character": "Player", "attribute": "Openness" }
```

### Relation scope

A directional attribute between two characters. `Mira→Player.Trust` and `Player→Mira.Trust` are independent values.

```json
{ "scope": "Relation", "from": "Mira", "to": "Player", "attribute": "Trust" }
```

### Scene scope

An attribute anchored to a specific scene (useful for atmosphere tracking).

```json
{ "scope": "Scene", "scene": "kitchen_confrontation", "attribute": "Tension" }
```

### World scope

A global value shared across all scenes.

```json
{ "scope": "World", "attribute": "DaysPassed" }
```

---

## Conditions

Conditions gate choice visibility. All condition objects require a `"$type"` field.

### AttributeCondition

Compares an attribute value to a threshold.

```json
{
  "$type": "AttributeCondition",
  "target": { "scope": "Character", "character": "Player", "attribute": "Openness" },
  "operator": "GreaterThan",
  "value": 5
}
```

| Field | Required | Description |
|-------|----------|-------------|
| `target` | yes | Attribute address to compare |
| `operator` | yes | `LessThan` / `LessThanOrEqual` / `Equal` / `GreaterThanOrEqual` / `GreaterThan` |
| `value` | yes | Integer threshold |
| `useEffectiveValue` | no | `true` (default) — includes modifier deltas; `false` — raw base value only |

### FlagCondition

Checks whether a flag has been set.

```json
{
  "$type": "FlagCondition",
  "flag": "BrokenVaseConfessed",
  "expected": true
}
```

| Field | Required | Description |
|-------|----------|-------------|
| `flag` | yes | Flag identifier — `PastTense` by convention |
| `expected` | yes | `true` = flag must be set; `false` = flag must not be set |

### AllCondition

Logical AND — all sub-conditions must be met.

```json
{
  "$type": "AllCondition",
  "conditions": [
    { "$type": "FlagCondition", "flag": "BrokenVaseConfessed", "expected": true },
    { "$type": "AttributeCondition", "target": { ... }, "operator": "GreaterThan", "value": 10 }
  ]
}
```

### AnyCondition

Logical OR — at least one sub-condition must be met.

```json
{
  "$type": "AnyCondition",
  "conditions": [
    { "$type": "FlagCondition", "flag": "ApologisedEarly", "expected": true },
    { "$type": "FlagCondition", "flag": "BroughtGift", "expected": true }
  ]
}
```

### NotCondition

Logical NOT — the sub-condition must not be met.

```json
{
  "$type": "NotCondition",
  "condition": { "$type": "FlagCondition", "flag": "LiedToMira", "expected": true }
}
```

---

## Effects

Effects mutate game state. All effect objects require a `"$type"` field.

### ChangeAttributeEffect

Adds `delta` to an attribute's base value (clamped to –100…+100).

```json
{
  "$type": "ChangeAttributeEffect",
  "target": { "scope": "Relation", "from": "Mira", "to": "Player", "attribute": "Trust" },
  "delta": 5
}
```

### SetAttributeEffect

Sets an attribute to an exact value (clamped to –100…+100).

```json
{
  "$type": "SetAttributeEffect",
  "target": { "scope": "Character", "character": "Player", "attribute": "Openness" },
  "value": 50
}
```

### SetFlagEffect

Marks a flag as having happened.

```json
{ "$type": "SetFlagEffect", "flag": "BrokenVaseConfessed" }
```

### ClearFlagEffect

Removes a flag.

```json
{ "$type": "ClearFlagEffect", "flag": "BrokenVaseConfessed" }
```

### MoveToSceneEffect

Transitions to a scene. Prefer `"nextScene"` on the choice for normal flow; use this inside a `ConditionalEffect` when the target must be decided at runtime.

```json
{ "$type": "MoveToSceneEffect", "scene": "epilogue" }
```

### ConditionalEffect

Branches between two effect lists at runtime based on a condition.

```json
{
  "$type": "ConditionalEffect",
  "condition": { "$type": "FlagCondition", "flag": "BrokenVaseConfessed", "expected": true },
  "then": [
    {
      "$type": "ChangeAttributeEffect",
      "target": { "scope": "Relation", "from": "Mira", "to": "Player", "attribute": "Trust" },
      "delta": 10
    }
  ],
  "else": [
    {
      "$type": "ChangeAttributeEffect",
      "target": { "scope": "Relation", "from": "Mira", "to": "Player", "attribute": "Trust" },
      "delta": -5
    }
  ]
}
```

The `"else"` list defaults to empty (no-op) if omitted.

---

## Ambiance

Attribute assignments that describe a scene's baseline atmosphere. These are informational metadata — they are **not** automatically applied to game state. To apply them to state, use `onEnterEffects` with `SetAttributeEffect` entries.

```json
"ambiance": [
  {
    "target": { "scope": "Scene", "scene": "kitchen_confrontation", "attribute": "Tension" },
    "value": 50
  }
]
```

---

## Modifiers

Modifiers temporarily adjust the *effective* value of an attribute without changing the stored base value. The engine applies them whenever it evaluates conditions in the current scene.

```json
"modifiers": [
  {
    "id": "tension_reduces_control",
    "target": { "scope": "Character", "character": "Player", "attribute": "Control" },
    "delta": -10,
    "duration": "CurrentScene"
  }
]
```

| Field | Required | Description |
|-------|----------|-------------|
| `id` | yes | Diagnostic name — `snake_case` |
| `target` | yes | Attribute to adjust |
| `delta` | yes | How much to add (negative = reduce) |
| `duration` | no | `CurrentScene` (default) or `Instant` |
| `condition` | no | Optional gate; **must only read base values** to avoid infinite recursion |

**`CurrentScene`** — active for as long as the player is in this scene. This is the most common use: a tense room that reduces everyone's self-control, a drunk NPC whose Perception is lowered.

**`Instant`** — applied once at scene entry and does not persist. In most cases `onEnterEffects` is a clearer choice.

---

## Initial State File (`_initial-state.json`)

Seeds attribute values and the starting scene before the first play begins. The engine loads this file when no save file exists and no `--start` argument is given.

```json
{
  "currentScene": "first_scene_id",
  "attributes": [
    {
      "address": { "scope": "Character", "character": "Player", "attribute": "Openness" },
      "value": 20
    },
    {
      "address": { "scope": "Relation", "from": "Mira", "to": "Player", "attribute": "Trust" },
      "value": 25
    }
  ],
  "flags": ["AlreadyMet"]
}
```

All attributes default to `0` if not listed. Flags default to empty.

---

## Common Patterns

### Linear scene flow

Every choice leads to exactly one next scene:

```json
{ "id": "continue", "text": "Continue.", "nextScene": "next_scene" }
```

### Branching by attribute

Show different choices based on how the story has unfolded:

```json
{
  "id": "persuade",
  "text": "\"Let me explain.\"",
  "condition": {
    "$type": "AttributeCondition",
    "target": { "scope": "Relation", "from": "Mira", "to": "Player", "attribute": "Trust" },
    "operator": "GreaterThanOrEqual",
    "value": 10
  },
  "nextScene": "mira_listens"
}
```

### Branching by flag

Gate a choice on a past decision:

```json
{
  "id": "remind_of_confession",
  "text": "\"Remember — I told you the truth.\"",
  "condition": { "$type": "FlagCondition", "flag": "Apologised", "expected": true },
  "nextScene": "reconciliation"
}
```

### Runtime branch on scene entry

Use `onEnterEffects` with `ConditionalEffect` to apply different consequences based on accumulated state, even when all players reach the same scene:

```json
"onEnterEffects": [
  {
    "$type": "ConditionalEffect",
    "condition": { "$type": "FlagCondition", "flag": "BrokenVaseConfessed", "expected": true },
    "then": [
      { "$type": "ChangeAttributeEffect", "target": { "scope": "Relation", "from": "Mira", "to": "Player", "attribute": "Trust" }, "delta": 5 }
    ]
  }
]
```

### Story ending

A scene with an empty (or all-gated) choices list ends the story:

```json
{
  "id": "epilogue",
  "title": "The End",
  "description": "Years passed. Things changed.",
  "choices": []
}
```
