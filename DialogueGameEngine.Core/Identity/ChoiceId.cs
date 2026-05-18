namespace DialogueGameEngine.Core;

/// <summary>
/// Uniquely identifies a player choice within a scene.
/// </summary>
/// <remarks>
/// Choice IDs are scoped to their scene — two different scenes can each have a choice
/// named <c>"refuse"</c> without conflict. Use short, action-describing slugs:
/// <c>"confess"</c>, <c>"lie"</c>, <c>"stay_silent"</c>.
/// </remarks>
/// <example>
/// <code>
/// engine.SelectChoice(state, new ChoiceId("confess"));
/// </code>
/// </example>
/// <param name="Value">The unique string identifier for this choice within its scene.</param>
public readonly record struct ChoiceId(string Value);
