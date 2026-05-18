namespace DialogueGameEngine.Core;

/// <summary>
/// Transitions the story to a different scene.
/// </summary>
/// <remarks>
/// <para>
/// For straightforward linear dialogue flow, prefer <see cref="ChoiceDefinition.NextScene"/>
/// instead — it is more readable in a scene definition.
/// Reserve <see cref="MoveToSceneEffect"/> for cases where the target scene depends on
/// runtime state and must be chosen inside a <see cref="ConditionalEffect"/>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Jump to a different scene conditionally
/// new ConditionalEffect
/// {
///     Condition = new FlagCondition { Flag = new FlagId("BrokenVaseConfessed") },
///     Then = [ new MoveToSceneEffect { Scene = new SceneId("mira_accepts_truth") } ],
///     Else = [ new MoveToSceneEffect { Scene = new SceneId("mira_suspicious") }   ]
/// }
/// </code>
/// </example>
public sealed record MoveToSceneEffect : IEffect
{
    /// <summary>The scene to transition to.</summary>
    public required SceneId Scene { get; init; }

    /// <inheritdoc/>
    public void Apply(GameState state, EvaluationContext context) =>
        state.MoveTo(Scene);
}
