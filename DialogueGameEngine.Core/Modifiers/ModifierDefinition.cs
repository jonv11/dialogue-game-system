namespace DialogueGameEngine.Core;

/// <summary>
/// Adjusts the <em>effective</em> value of an attribute without permanently changing the base state.
/// </summary>
/// <remarks>
/// <para>
/// Modifiers are for context-sensitive influence: a tense room that makes everyone less controlled,
/// a drunk NPC whose perception is reduced, a suspicious guard who evaluates all actions more harshly.
/// </para>
/// <para>
/// The distinction between base and effective value is fundamental:
/// <list type="bullet">
///   <item><b>Base value</b> — the raw number stored in <see cref="GameState"/>.</item>
///   <item><b>Effective value</b> — base value plus all active modifier deltas.</item>
/// </list>
/// Conditions using <see cref="AttributeCondition.UseEffectiveValue"/> (the default) compare
/// against the effective value, so modifiers automatically influence what choices are available.
/// </para>
/// <para>
/// To make a permanent state change, use <see cref="IEffect"/> instead.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // A tense scene that reduces the player's ability to stay calm
/// new ModifierDefinition
/// {
///     Id       = new ModifierId("tense_room_reduces_control"),
///     Target   = AttributeAddress.CharacterAttribute(new CharacterId("Player"), new AttributeId("Control")),
///     Delta    = -10,
///     Duration = ModifierDuration.CurrentScene
/// }
/// </code>
/// </example>
public sealed record ModifierDefinition
{
    /// <summary>A unique identifier used for diagnostics.</summary>
    public required ModifierId Id { get; init; }

    /// <summary>The attribute whose effective value this modifier adjusts.</summary>
    public required AttributeAddress Target { get; init; }

    /// <summary>
    /// How much to add to the attribute's base value when computing the effective value.
    /// Negative values reduce the attribute; positive values raise it.
    /// </summary>
    public required int Delta { get; init; }

    /// <summary>How long the modifier remains active. Defaults to <see cref="ModifierDuration.CurrentScene"/>.</summary>
    public ModifierDuration Duration { get; init; } = ModifierDuration.CurrentScene;

    /// <summary>
    /// An optional condition that gates this modifier. When set, the delta is only applied
    /// if the condition is true at evaluation time.
    /// <para>
    /// <b>Important:</b> This condition must only read base values via
    /// <see cref="EvaluationContext.GetBaseValue"/>. Using <see cref="EvaluationContext.GetEffectiveValue"/>
    /// inside a modifier condition causes infinite recursion.
    /// </para>
    /// </summary>
    public ICondition? Condition { get; init; }
}
