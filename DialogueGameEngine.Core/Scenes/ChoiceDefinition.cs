namespace DialogueGameEngine.Core;

/// <summary>
/// A player-facing option within a scene, with an optional visibility condition,
/// a list of effects, and an optional scene to transition to.
/// </summary>
/// <remarks>
/// <para>
/// Choices are the primary way the player shapes the story. When selected, their
/// scene's exit effects run, then <see cref="Effects"/> are applied in order, then the engine
/// moves to <see cref="NextScene"/> (if set) and runs the target scene's enter effects.
/// </para>
/// <para>
/// A <see cref="Condition"/> of <see langword="null"/> means the choice is always available.
/// The engine evaluates conditions against effective attribute values by default, so scene
/// modifiers can gate choices dynamically without touching base state.
/// </para>
/// <para>
/// Prefer <see cref="NextScene"/> for normal dialogue flow.
/// Use <see cref="MoveToSceneEffect"/> inside <see cref="Effects"/> only when the target scene
/// must be determined conditionally at runtime.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// new ChoiceDefinition
/// {
///     Id        = new ChoiceId("confess"),
///     Text      = "Yes. I broke it.",
///
///     Condition = new AttributeCondition
///     {
///         Target   = AttributeAddress.CharacterAttribute(player, openness),
///         Operator = ComparisonOperator.GreaterThan,
///         Value    = 5
///     },
///
///     Effects =
///     [
///         new ChangeAttributeEffect { Target = AttributeAddress.CharacterAttribute(player, openness), Delta = 2 },
///         new ChangeAttributeEffect { Target = AttributeAddress.RelationAttribute(mira, player, trust), Delta = 5 },
///         new SetFlagEffect          { Flag = new FlagId("BrokenVaseConfessed") }
///     ],
///
///     NextScene = new SceneId("mira_accepts_truth")
/// }
/// </code>
/// </example>
public sealed record ChoiceDefinition
{
    /// <summary>The unique identifier of this choice within its scene.</summary>
    public required ChoiceId Id { get; init; }

    /// <summary>The text the player sees when this choice is offered.</summary>
    public required string Text { get; init; }

    /// <summary>
    /// Optional condition that must be satisfied for this choice to appear.
    /// When <see langword="null"/>, the choice is always available.
    /// </summary>
    public ICondition? Condition { get; init; }

    /// <summary>Effects applied in order when the player selects this choice.</summary>
    public IReadOnlyList<IEffect> Effects { get; init; } = [];

    /// <summary>
    /// The scene to move to after this choice's effects are applied.
    /// Use <see langword="null"/> to stay in the current scene (rare — for looping dialogue).
    /// </summary>
    public SceneId? NextScene { get; init; }

    /// <summary>
    /// Returns <see langword="true"/> if this choice's condition is satisfied (or has no condition).
    /// </summary>
    /// <param name="context">The current evaluation context.</param>
    public bool IsAvailable(EvaluationContext context) =>
        Condition is null || Condition.IsMet(context);
}
