namespace DialogueGameEngine.Core;

/// <summary>
/// Applies one of two effect lists depending on whether a condition is met at runtime.
/// </summary>
/// <remarks>
/// <para>
/// Use this for story beats that branch based on state accumulated earlier in the session.
/// For example, you can grant bonus trust only to players who confessed earlier,
/// even if both the honest and dishonest player reach the same scene.
/// </para>
/// <para>
/// If <see cref="Condition"/> is met, the <see cref="Then"/> effects are applied.
/// Otherwise the <see cref="Else"/> effects are applied (empty by default — a no-op).
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Award bonus trust if the player previously confessed
/// new ConditionalEffect
/// {
///     Condition = new FlagCondition { Flag = new FlagId("BrokenVaseConfessed") },
///     Then =
///     [
///         new ChangeAttributeEffect
///         {
///             Target = AttributeAddress.RelationAttribute(mira, player, trust),
///             Delta  = 10
///         }
///     ],
///     Else =
///     [
///         new ChangeAttributeEffect
///         {
///             Target = AttributeAddress.RelationAttribute(mira, player, trust),
///             Delta  = -5
///         }
///     ]
/// }
/// </code>
/// </example>
public sealed record ConditionalEffect : IEffect
{
    /// <summary>The condition that determines which branch to execute.</summary>
    public required ICondition Condition { get; init; }

    /// <summary>Effects applied when <see cref="Condition"/> is satisfied.</summary>
    public required IReadOnlyList<IEffect> Then { get; init; }

    /// <summary>
    /// Effects applied when <see cref="Condition"/> is not satisfied. Empty by default (no-op).
    /// </summary>
    public IReadOnlyList<IEffect> Else { get; init; } = [];

    /// <inheritdoc/>
    public void Apply(GameState state, EvaluationContext context)
    {
        var branch = Condition.IsMet(context) ? Then : Else;

        foreach (var effect in branch)
            effect.Apply(state, context);
    }
}
