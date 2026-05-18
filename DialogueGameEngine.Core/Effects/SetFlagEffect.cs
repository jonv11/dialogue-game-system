namespace DialogueGameEngine.Core;

/// <summary>
/// Marks a story flag as having occurred — a permanent record that something happened.
/// </summary>
/// <remarks>
/// Flags represent decisive moments: a confession, a discovery, a broken promise.
/// Once set, a flag persists for the rest of the play session.
/// Use <see cref="FlagCondition"/> to gate future choices on whether the flag is set.
/// </remarks>
/// <example>
/// <code>
/// // Record that the player admitted to breaking the vase
/// new SetFlagEffect { Flag = new FlagId("BrokenVaseConfessed") }
/// </code>
/// </example>
public sealed record SetFlagEffect : IEffect
{
    /// <summary>The flag to set.</summary>
    public required FlagId Flag { get; init; }

    /// <inheritdoc/>
    public void Apply(GameState state, EvaluationContext context) =>
        state.SetFlag(Flag);
}
