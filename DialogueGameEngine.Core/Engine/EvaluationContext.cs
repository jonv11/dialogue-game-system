namespace DialogueGameEngine.Core;

/// <summary>
/// A read-only view of the current story state used by conditions and modifier evaluation.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="EvaluationContext"/> is created fresh by <see cref="DialogueEngine"/> every time
/// conditions need to be checked. It provides two ways to read an attribute:
/// </para>
/// <list type="bullet">
///   <item>
///     <see cref="GetBaseValue"/> — the raw value stored in <see cref="GameState"/>.
///     Use this when you need to test what was explicitly set by effects.
///   </item>
///   <item>
///     <see cref="GetEffectiveValue"/> — base value plus all applicable active modifier deltas.
///     This is what conditions use by default and reflects the full situational context.
///   </item>
/// </list>
/// <para>
/// <b>Mutation is not possible through this type.</b> Only <see cref="IEffect"/> implementations
/// receive a writable <see cref="GameState"/> reference.
/// </para>
/// </remarks>
public sealed class EvaluationContext
{
    /// <summary>The live game state (read through this context, mutated only by effects).</summary>
    public required GameState State { get; init; }

    /// <summary>The scene currently being evaluated.</summary>
    public required SceneDefinition Scene { get; init; }

    /// <summary>The modifiers that are currently in force for this scene.</summary>
    public required IReadOnlyList<ModifierDefinition> ActiveModifiers { get; init; }

    /// <summary>
    /// Returns the raw base value of <paramref name="address"/> as stored in game state.
    /// Modifier deltas are not included.
    /// </summary>
    /// <param name="address">The attribute to read.</param>
    public AttributeValue GetBaseValue(AttributeAddress address) =>
        State.GetAttribute(address);

    /// <summary>
    /// Returns the effective value of <paramref name="address"/>: the base value plus the sum
    /// of all applicable active modifier deltas. This is the value that most conditions compare against.
    /// </summary>
    /// <param name="address">The attribute to read.</param>
    /// <remarks>
    /// <b>Important:</b> modifier conditions are evaluated here using this same context.
    /// A modifier's <see cref="ModifierDefinition.Condition"/> must only call
    /// <see cref="GetBaseValue"/> — calling <see cref="GetEffectiveValue"/> from inside a
    /// modifier condition causes infinite recursion.
    /// </remarks>
    public AttributeValue GetEffectiveValue(AttributeAddress address)
    {
        var baseValue = State.GetAttribute(address).Value;

        // IMPORTANT: modifier conditions evaluated here must only read base values.
        // A modifier condition that calls GetEffectiveValue would cause infinite recursion.
        var modifierDelta = ActiveModifiers
            .Where(m => m.Target == address)
            .Where(m => m.Condition is null || m.Condition.IsMet(this))
            .Sum(m => m.Delta);

        return new AttributeValue(baseValue + modifierDelta);
    }

    /// <summary>Returns <see langword="true"/> if <paramref name="flag"/> is currently set in game state.</summary>
    public bool HasFlag(FlagId flag) => State.HasFlag(flag);
}
