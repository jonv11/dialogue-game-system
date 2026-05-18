namespace DialogueGameEngine.Core;

/// <summary>
/// A condition that compares an attribute value to a fixed threshold using a <see cref="ComparisonOperator"/>.
/// </summary>
/// <remarks>
/// <para>
/// By default the comparison uses the <em>effective</em> value — the base value plus any active
/// modifier deltas. Set <see cref="UseEffectiveValue"/> to <see langword="false"/> to compare
/// the raw base value instead, ignoring modifiers.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Show the "confess" option only when Player.Openness > 5
/// var condition = new AttributeCondition
/// {
///     Target   = AttributeAddress.CharacterAttribute(new CharacterId("Player"), new AttributeId("Openness")),
///     Operator = ComparisonOperator.GreaterThan,
///     Value    = 5
/// };
/// </code>
/// </example>
public sealed record AttributeCondition : ICondition
{
    /// <summary>The attribute to compare.</summary>
    public required AttributeAddress Target { get; init; }

    /// <summary>How to compare the attribute value against <see cref="Value"/>.</summary>
    public required ComparisonOperator Operator { get; init; }

    /// <summary>The threshold value to compare against.</summary>
    public required int Value { get; init; }

    /// <summary>
    /// When <see langword="true"/> (the default), the effective value — base plus modifier deltas — is used.
    /// Set to <see langword="false"/> to test the raw base value and ignore modifiers.
    /// </summary>
    public bool UseEffectiveValue { get; init; } = true;

    /// <inheritdoc/>
    public bool IsMet(EvaluationContext context)
    {
        var actual = UseEffectiveValue
            ? context.GetEffectiveValue(Target).Value
            : context.GetBaseValue(Target).Value;

        return Operator switch
        {
            ComparisonOperator.LessThan           => actual < Value,
            ComparisonOperator.LessThanOrEqual    => actual <= Value,
            ComparisonOperator.Equal              => actual == Value,
            ComparisonOperator.GreaterThanOrEqual => actual >= Value,
            ComparisonOperator.GreaterThan        => actual > Value,
            _ => throw new InvalidOperationException($"Unsupported operator: {Operator}")
        };
    }
}
