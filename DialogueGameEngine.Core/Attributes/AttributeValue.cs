namespace DialogueGameEngine.Core;

/// <summary>
/// A numeric attribute value clamped to the range [-100, 100].
/// </summary>
/// <remarks>
/// <para>
/// All story attributes — trust, tension, openness, fear — live in this range.
/// Think of 0 as neutral, positive values as favorable, and negative values as unfavorable.
/// </para>
/// <para>
/// Values are clamped on construction rather than throwing, so a misconfigured
/// effect that overshoots the range degrades gracefully instead of crashing mid-story.
/// </para>
/// </remarks>
public readonly record struct AttributeValue
{
    /// <summary>The minimum allowed value (-100).</summary>
    public const int Min = -100;

    /// <summary>The maximum allowed value (100).</summary>
    public const int Max = 100;

    /// <summary>The raw integer value, always in [<see cref="Min"/>, <see cref="Max"/>].</summary>
    public int Value { get; }

    /// <summary>
    /// Creates an <see cref="AttributeValue"/>, clamping <paramref name="value"/> to [-100, 100].
    /// </summary>
    /// <param name="value">The desired value. Values outside the range are silently clamped.</param>
    public AttributeValue(int value)
    {
        // Clamping rather than throwing keeps the engine alive through bad data.
        // A misconfigured modifier that overshoots the range should degrade gracefully,
        // not crash a play session.
        Value = Math.Clamp(value, Min, Max);
    }

    /// <summary>
    /// Returns a new <see cref="AttributeValue"/> with <paramref name="delta"/> added, clamped.
    /// </summary>
    /// <param name="delta">How much to add (negative values decrease the attribute).</param>
    public AttributeValue Add(int delta) => new(Value + delta);

    /// <summary>Allows using an <see cref="AttributeValue"/> wherever an <see langword="int"/> is expected.</summary>
    public static implicit operator int(AttributeValue value) => value.Value;
}
