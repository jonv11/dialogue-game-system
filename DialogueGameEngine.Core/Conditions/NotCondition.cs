namespace DialogueGameEngine.Core;

/// <summary>
/// A condition that inverts another condition (logical NOT).
/// </summary>
/// <example>
/// <code>
/// // Block a choice unless Mira's suspicion is NOT already at maximum
/// new NotCondition
/// {
///     Condition = new AttributeCondition
///     {
///         Target   = AttributeAddress.CharacterAttribute(mira, suspicion),
///         Operator = ComparisonOperator.Equal,
///         Value    = 100
///     }
/// }
/// </code>
/// </example>
public sealed record NotCondition : ICondition
{
    /// <summary>The condition whose result is negated.</summary>
    public required ICondition Condition { get; init; }

    /// <inheritdoc/>
    public bool IsMet(EvaluationContext context) => !Condition.IsMet(context);
}
