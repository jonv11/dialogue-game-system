namespace DialogueGameEngine.Core;

/// <summary>
/// A composite condition that is met when <em>at least one</em> sub-condition is met (logical OR).
/// </summary>
/// <remarks>
/// An empty <see cref="Conditions"/> list returns <see langword="false"/>.
/// Use this to allow multiple alternate paths to unlock the same choice or effect.
/// </remarks>
/// <example>
/// <code>
/// // Show a sympathy option if the player is either very open OR has already confessed
/// new AnyCondition
/// {
///     Conditions =
///     [
///         new AttributeCondition { Target = ..., Operator = ComparisonOperator.GreaterThan, Value = 50 },
///         new FlagCondition { Flag = new FlagId("BrokenVaseConfessed") }
///     ]
/// }
/// </code>
/// </example>
public sealed record AnyCondition : ICondition
{
    /// <summary>The sub-conditions, of which at least one must be satisfied.</summary>
    public required IReadOnlyList<ICondition> Conditions { get; init; }

    /// <inheritdoc/>
    public bool IsMet(EvaluationContext context) =>
        Conditions.Any(c => c.IsMet(context));
}
