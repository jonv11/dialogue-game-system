namespace DialogueGameEngine.Core;

/// <summary>
/// Adds a signed delta to an attribute, keeping the result clamped to [-100, 100].
/// </summary>
/// <remarks>
/// This is the most common effect in a dialogue system — building or eroding trust,
/// raising or lowering tension, reflecting the emotional consequence of a player choice.
/// </remarks>
/// <example>
/// <code>
/// // Confessing raises Mira's trust in the player by 5 points
/// new ChangeAttributeEffect
/// {
///     Target = AttributeAddress.RelationAttribute(mira, player, trust),
///     Delta  = 5
/// }
///
/// // Lying lowers it by 5
/// new ChangeAttributeEffect
/// {
///     Target = AttributeAddress.RelationAttribute(mira, player, trust),
///     Delta  = -5
/// }
/// </code>
/// </example>
public sealed record ChangeAttributeEffect : IEffect
{
    /// <summary>The attribute to modify.</summary>
    public required AttributeAddress Target { get; init; }

    /// <summary>
    /// How much to add to the attribute. Use a positive number to increase, negative to decrease.
    /// The result is automatically clamped to [-100, 100].
    /// </summary>
    public required int Delta { get; init; }

    /// <inheritdoc/>
    public void Apply(GameState state, EvaluationContext context) =>
        state.AddToAttribute(Target, Delta);
}
