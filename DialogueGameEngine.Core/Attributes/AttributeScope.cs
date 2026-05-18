namespace DialogueGameEngine.Core;

/// <summary>
/// Defines who or what owns an attribute value.
/// Used by <see cref="AttributeAddress"/> to route attribute reads and writes to the right place.
/// </summary>
public enum AttributeScope
{
    /// <summary>
    /// The attribute belongs to a single character.
    /// Example: <c>Player.Openness</c>, <c>Mira.Courage</c>.
    /// </summary>
    Character,

    /// <summary>
    /// The attribute belongs to a directed relationship between two characters.
    /// Example: <c>Mira → Player.Trust</c> (how much Mira trusts the player).
    /// Note that <c>Mira → Player.Trust</c> and <c>Player → Mira.Trust</c> are separate attributes.
    /// </summary>
    Relation,

    /// <summary>
    /// The attribute belongs to a specific scene.
    /// Example: <c>kitchen_argument.Tension</c>.
    /// Scene attributes are typically set via <see cref="SceneDefinition.Ambiance"/>.
    /// </summary>
    Scene,

    /// <summary>
    /// The attribute belongs to the world — shared global state.
    /// Example: <c>World.AuthorityPressure</c>, <c>World.DaysSinceIncident</c>.
    /// </summary>
    World
}
