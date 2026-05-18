namespace DialogueGameEngine.Core;

/// <summary>
/// Determines whether a story rule or player choice is currently satisfied.
/// </summary>
/// <remarks>
/// <para>
/// Conditions are read-only — they never change game state.
/// They are evaluated by the engine to decide which choices are visible,
/// and by <see cref="ConditionalEffect"/> to decide which branch of an effect to apply.
/// </para>
/// <para>
/// Built-in implementations:
/// <list type="bullet">
///   <item><see cref="AttributeCondition"/> — compares an attribute to a threshold</item>
///   <item><see cref="FlagCondition"/> — checks whether a flag is set or absent</item>
///   <item><see cref="AllCondition"/> — all sub-conditions must be true (AND)</item>
///   <item><see cref="AnyCondition"/> — at least one sub-condition must be true (OR)</item>
///   <item><see cref="NotCondition"/> — negates a single condition</item>
/// </list>
/// </para>
/// </remarks>
public interface ICondition
{
    /// <summary>
    /// Returns <see langword="true"/> if this condition is currently satisfied.
    /// </summary>
    /// <param name="context">
    /// The evaluation context providing read access to game state, active modifiers,
    /// and the current scene.
    /// </param>
    bool IsMet(EvaluationContext context);
}
