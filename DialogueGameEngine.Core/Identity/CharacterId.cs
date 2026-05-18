namespace DialogueGameEngine.Core;

/// <summary>
/// Uniquely identifies a character in the story — player, NPC, or narrator.
/// </summary>
/// <remarks>
/// Use a stable, human-readable string as the value. This ID is referenced everywhere
/// character-specific attributes and relations are tracked.
/// </remarks>
/// <example>
/// <code>
/// var player = new CharacterId("Player");
/// var mira   = new CharacterId("Mira");
/// </code>
/// </example>
/// <param name="Value">The unique string identifier for this character.</param>
public readonly record struct CharacterId(string Value);
