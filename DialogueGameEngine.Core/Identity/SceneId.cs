namespace DialogueGameEngine.Core;

/// <summary>
/// Uniquely identifies a scene — a single unit of narrative context in which dialogue happens.
/// </summary>
/// <remarks>
/// Use a snake_case string that describes the moment: <c>"broken_vase_confrontation"</c>,
/// <c>"market_haggling"</c>. The engine uses this ID to look up scene definitions and
/// to record which scene the player is currently in.
/// </remarks>
/// <example>
/// <code>
/// var scene = new SceneId("broken_vase_confrontation");
/// </code>
/// </example>
/// <param name="Value">The unique string identifier for this scene.</param>
public readonly record struct SceneId(string Value);
