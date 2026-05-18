namespace DialogueGameEngine.Core;

/// <summary>
/// Specifies how long a <see cref="ModifierDefinition"/> remains active.
/// </summary>
public enum ModifierDuration
{
    /// <summary>
    /// The modifier is applied once at scene entry and does not persist as an active modifier.
    /// In most cases you should use a <see cref="SceneDefinition.OnEnterEffects"/> entry instead.
    /// </summary>
    Instant,

    /// <summary>
    /// The modifier is active for as long as the player is in the scene that defines it.
    /// This is the default and the most common duration.
    /// Example: a tense room that reduces everyone's self-control while the scene plays out.
    /// </summary>
    CurrentScene,

    /// <summary>
    /// The modifier persists across scene transitions until it is explicitly removed.
    /// <para><b>Note:</b> cross-scene tracking is not yet implemented in <see cref="DialogueEngine"/>.
    /// This value is reserved for a future storage layer in <see cref="GameState"/>.</para>
    /// </summary>
    UntilCleared,

    /// <summary>
    /// The modifier is always active regardless of the current scene.
    /// <para><b>Note:</b> cross-scene tracking is not yet implemented in <see cref="DialogueEngine"/>.
    /// This value is reserved for a future storage layer in <see cref="GameState"/>.</para>
    /// </summary>
    Permanent
}
