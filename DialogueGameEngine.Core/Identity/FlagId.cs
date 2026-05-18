namespace DialogueGameEngine.Core;

/// <summary>
/// Names a boolean story flag — something that either has or has not happened.
/// </summary>
/// <remarks>
/// <para>
/// Flags are the simplest form of state: they are either present (true) or absent (false).
/// Use them to record decisive story moments: confessions, discoveries, broken promises.
/// </para>
/// <para>
/// Use PastTense names that describe a completed event:
/// <c>"BrokenVaseConfessed"</c>, <c>"GuiltySecretRevealed"</c>, <c>"AllianceForged"</c>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// state.SetFlag(new FlagId("BrokenVaseConfessed"));
///
/// if (state.HasFlag(new FlagId("BrokenVaseConfessed")))
///     // player admitted guilt — Mira is more forgiving
/// </code>
/// </example>
/// <param name="Value">The name of the flag (e.g. <c>"BrokenVaseConfessed"</c>).</param>
public readonly record struct FlagId(string Value);
