namespace DialogueGameEngine.Core;

/// <summary>
/// Uniquely identifies a modifier definition — a temporary adjustment to an attribute's effective value.
/// </summary>
/// <remarks>
/// Modifier IDs are used for diagnostics and debugging. Use descriptive snake_case names
/// that explain why the modifier exists: <c>"tense_room_reduces_control"</c>,
/// <c>"drunk_npc_lowers_perception"</c>.
/// </remarks>
/// <param name="Value">The unique string identifier for this modifier.</param>
public readonly record struct ModifierId(string Value);
