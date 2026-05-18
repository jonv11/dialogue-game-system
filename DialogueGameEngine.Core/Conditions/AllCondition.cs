namespace DialogueGameEngine.Core;

/// <summary>
/// A composite condition that is met only when <em>every</em> sub-condition is met (logical AND).
/// </summary>
/// <remarks>
/// An empty <see cref="Conditions"/> list returns <see langword="true"/> (vacuous truth).
/// Use this to combine multiple requirements that must all hold simultaneously.
/// </remarks>
/// <example>
/// <code>
/// // Require both high openness AND the confession flag
/// new AllCondition
/// {
///     Conditions =
///     [
///         new AttributeCondition { Target = ..., Operator = ComparisonOperator.GreaterThan, Value = 5 },
///         new FlagCondition { Flag = new FlagId("BrokenVaseConfessed") }
///     ]
/// }
/// </code>
/// </example>
public sealed record AllCondition : ICondition
{
    /// <summary>The sub-conditions that must all be satisfied.</summary>
    public required IReadOnlyList<ICondition> Conditions { get; init; }

    /// <inheritdoc/>
    public bool IsMet(EvaluationContext context) =>
        Conditions.All(c => c.IsMet(context));
}
