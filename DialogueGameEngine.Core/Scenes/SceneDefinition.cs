namespace DialogueGameEngine.Core;

/// <summary>
/// The complete definition of a single scene — a unit of narrative context in which dialogue unfolds.
/// </summary>
/// <remarks>
/// <para>
/// A scene is both a place and a moment. It holds the characters present, the emotional
/// atmosphere (ambiance), any temporary modifier effects, and the choices the player can make.
/// </para>
/// <para>
/// Scenes are immutable definitions loaded once at engine startup. Runtime state —
/// which scene the player is currently in, what attributes have changed — lives in <see cref="GameState"/>.
/// </para>
/// <para>
/// <b>Lifecycle:</b>
/// When the engine moves to a scene, it executes <see cref="OnEnterEffects"/>.
/// When it leaves, it executes <see cref="OnExitEffects"/> (you are responsible for calling these
/// via your runner — <see cref="DialogueEngine"/> provides the building blocks).
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var scene = new SceneDefinition
/// {
///     Id          = new SceneId("broken_vase_confrontation"),
///     Title       = "Broken Vase Confrontation",
///     Description = "Mira confronts the player about the broken vase.",
///     Participants = [new CharacterId("Player"), new CharacterId("Mira")],
///
///     Ambiance =
///     [
///         new AttributeAssignment
///         {
///             Target = AttributeAddress.SceneAttribute(
///                 new SceneId("broken_vase_confrontation"), new AttributeId("Tension")),
///             Value = 50
///         }
///     ],
///
///     Choices =
///     [
///         new ChoiceDefinition { Id = new ChoiceId("confess"), Text = "Yes. I broke it.", ... },
///         new ChoiceDefinition { Id = new ChoiceId("lie"),     Text = "No. It was already like that.", ... }
///     ]
/// };
/// </code>
/// </example>
public sealed record SceneDefinition
{
    /// <summary>The unique identifier for this scene.</summary>
    public required SceneId Id { get; init; }

    /// <summary>A short title displayed to the player at the top of the scene.</summary>
    public required string Title { get; init; }

    /// <summary>A longer description of the scene's setting and opening situation. Optional.</summary>
    public string? Description { get; init; }

    /// <summary>
    /// The characters present in this scene.
    /// This is informational — the engine does not enforce participation constraints.
    /// </summary>
    public IReadOnlyList<CharacterId> Participants { get; init; } = [];

    /// <summary>
    /// Static attribute assignments that describe the scene's baseline atmosphere.
    /// For example: <c>Tension = 50</c>, <c>LightLevel = -20</c>.
    /// These values are not automatically applied to <see cref="GameState"/> —
    /// use <see cref="OnEnterEffects"/> with <see cref="SetAttributeEffect"/> entries if you want
    /// them reflected in live state.
    /// </summary>
    public IReadOnlyList<AttributeAssignment> Ambiance { get; init; } = [];

    /// <summary>
    /// Temporary modifiers that adjust effective attribute values while this scene is active.
    /// Only <see cref="ModifierDuration.CurrentScene"/> modifiers are picked up by the engine.
    /// </summary>
    public IReadOnlyList<ModifierDefinition> Modifiers { get; init; } = [];

    /// <summary>All player choices available in this scene, subject to each choice's condition.</summary>
    public IReadOnlyList<ChoiceDefinition> Choices { get; init; } = [];

    /// <summary>
    /// Effects applied when the player enters this scene.
    /// Use these to apply ambiance attributes to <see cref="GameState"/> or trigger story beats.
    /// </summary>
    public IReadOnlyList<IEffect> OnEnterEffects { get; init; } = [];

    /// <summary>
    /// Effects applied when the player leaves this scene (before the next scene is loaded).
    /// Use these for cleanup or trailing consequences.
    /// </summary>
    public IReadOnlyList<IEffect> OnExitEffects { get; init; } = [];
}
