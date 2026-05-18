namespace DialogueGameEngine.Core;

/// <summary>
/// A static attribute value baked into a scene definition — primarily used to describe ambiance.
/// </summary>
/// <remarks>
/// <para>
/// Place <see cref="AttributeAssignment"/> entries in <see cref="SceneDefinition.Ambiance"/> to
/// declare the emotional or atmospheric baseline of a scene. The CLI and tools can display
/// these values. They are not read by conditions unless an effect first writes the value into
/// <see cref="GameState"/>.
/// </para>
/// <para>
/// This is declarative data, not logic. To change an attribute value during play,
/// use <see cref="ChangeAttributeEffect"/> or <see cref="SetAttributeEffect"/> instead.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// new AttributeAssignment
/// {
///     Target = AttributeAddress.SceneAttribute(
///         new SceneId("kitchen_argument"),
///         new AttributeId("Tension")),
///     Value = 60  // this scene starts at high tension
/// }
/// </code>
/// </example>
public sealed record AttributeAssignment
{
    /// <summary>Which attribute this assignment targets.</summary>
    public required AttributeAddress Target { get; init; }

    /// <summary>The initial value to assign. Will be clamped to [-100, 100].</summary>
    public required int Value { get; init; }
}
