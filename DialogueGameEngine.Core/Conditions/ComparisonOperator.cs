namespace DialogueGameEngine.Core;

/// <summary>
/// The comparison to apply when evaluating an <see cref="AttributeCondition"/>.
/// </summary>
/// <example>
/// <code>
/// // Gate a "confess" choice on the player having at least some openness:
/// new AttributeCondition
/// {
///     Target   = AttributeAddress.CharacterAttribute(player, openness),
///     Operator = ComparisonOperator.GreaterThan,
///     Value    = 5
/// }
/// </code>
/// </example>
public enum ComparisonOperator
{
    /// <summary>Condition is met when the attribute value is strictly less than the threshold.</summary>
    LessThan,

    /// <summary>Condition is met when the attribute value is less than or equal to the threshold.</summary>
    LessThanOrEqual,

    /// <summary>Condition is met when the attribute value exactly equals the threshold.</summary>
    Equal,

    /// <summary>Condition is met when the attribute value is greater than or equal to the threshold.</summary>
    GreaterThanOrEqual,

    /// <summary>Condition is met when the attribute value is strictly greater than the threshold.</summary>
    GreaterThan
}
