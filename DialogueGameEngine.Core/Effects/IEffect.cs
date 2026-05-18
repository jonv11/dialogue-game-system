namespace DialogueGameEngine.Core;

/// <summary>
/// Mutates game state when a choice is selected or a scene is entered or exited.
/// </summary>
/// <remarks>
/// <para>
/// Effects are the only way to change <see cref="GameState"/>. Conditions only read state;
/// effects write it. This separation keeps narrative logic predictable.
/// </para>
/// <para>
/// <c>Effect</c> is used instead of <c>Action</c> throughout the engine to avoid
/// colliding with <see cref="System.Action"/> from the .NET base class library.
/// </para>
/// <para>
/// Built-in implementations:
/// <list type="bullet">
///   <item><see cref="ChangeAttributeEffect"/> — add or subtract from an attribute</item>
///   <item><see cref="SetAttributeEffect"/> — set an attribute to an exact value</item>
///   <item><see cref="SetFlagEffect"/> — mark an event as having happened</item>
///   <item><see cref="ClearFlagEffect"/> — unmark an event</item>
///   <item><see cref="MoveToSceneEffect"/> — jump to a different scene</item>
///   <item><see cref="ConditionalEffect"/> — branch between two effect lists at runtime</item>
/// </list>
/// </para>
/// </remarks>
public interface IEffect
{
    /// <summary>
    /// Applies this effect, mutating <paramref name="state"/> as appropriate.
    /// </summary>
    /// <param name="state">The game state to mutate.</param>
    /// <param name="context">
    /// Read-only evaluation context, available for conditional logic within the effect.
    /// </param>
    void Apply(GameState state, EvaluationContext context);
}
