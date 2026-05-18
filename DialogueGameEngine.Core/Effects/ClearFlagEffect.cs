namespace DialogueGameEngine.Core;

/// <summary>
/// Removes a story flag, as if the corresponding event never happened.
/// </summary>
/// <remarks>
/// Rarely needed in straightforward stories, but useful for branching narratives where
/// a retcon or reversal is part of the story design (e.g. the player denies a confession
/// later in the game through some dramatic turn).
/// Clearing a flag that is not set is a silent no-op.
/// </remarks>
/// <example>
/// <code>
/// new ClearFlagEffect { Flag = new FlagId("BrokenVaseConfessed") }
/// </code>
/// </example>
public sealed record ClearFlagEffect : IEffect
{
    /// <summary>The flag to remove.</summary>
    public required FlagId Flag { get; init; }

    /// <inheritdoc/>
    public void Apply(GameState state, EvaluationContext context) =>
        state.ClearFlag(Flag);
}
