namespace DialogueGameEngine.Core;

/// <summary>
/// A condition that checks whether a story flag is currently set (or absent).
/// </summary>
/// <remarks>
/// Use this to gate choices or effects on decisive story events that have already happened.
/// Set <see cref="Expected"/> to <see langword="false"/> to require that the flag is <em>not</em> set.
/// </remarks>
/// <example>
/// <code>
/// // Only show the "forgiveness" option after the player has confessed
/// new FlagCondition { Flag = new FlagId("BrokenVaseConfessed"), Expected = true }
///
/// // Show a suspicious line only if the player has NOT yet come clean
/// new FlagCondition { Flag = new FlagId("BrokenVaseConfessed"), Expected = false }
/// </code>
/// </example>
public sealed record FlagCondition : ICondition
{
    /// <summary>The flag to check.</summary>
    public required FlagId Flag { get; init; }

    /// <summary>
    /// The expected state of the flag. <see langword="true"/> (default) means the flag must be set.
    /// <see langword="false"/> means the flag must be absent.
    /// </summary>
    public bool Expected { get; init; } = true;

    /// <inheritdoc/>
    public bool IsMet(EvaluationContext context) => context.HasFlag(Flag) == Expected;
}
