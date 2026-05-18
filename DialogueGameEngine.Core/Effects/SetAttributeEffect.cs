namespace DialogueGameEngine.Core;

/// <summary>
/// Sets an attribute to an exact value, replacing whatever it was before.
/// </summary>
/// <remarks>
/// Use this when you need to hard-reset an attribute to a known baseline —
/// for example, resetting scene tension at the start of a new chapter.
/// For incremental changes prefer <see cref="ChangeAttributeEffect"/>.
/// </remarks>
/// <example>
/// <code>
/// // Reset world pressure to neutral when the crisis is resolved
/// new SetAttributeEffect
/// {
///     Target = AttributeAddress.WorldAttribute(new AttributeId("AuthorityPressure")),
///     Value  = 0
/// }
/// </code>
/// </example>
public sealed record SetAttributeEffect : IEffect
{
    /// <summary>The attribute to set.</summary>
    public required AttributeAddress Target { get; init; }

    /// <summary>The value to assign. Will be clamped to [-100, 100].</summary>
    public required int Value { get; init; }

    /// <inheritdoc/>
    public void Apply(GameState state, EvaluationContext context) =>
        state.SetAttribute(Target, Value);
}
