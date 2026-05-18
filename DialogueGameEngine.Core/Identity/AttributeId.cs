namespace DialogueGameEngine.Core;

/// <summary>
/// Names a numeric attribute such as <c>"Trust"</c>, <c>"Openness"</c>, or <c>"Tension"</c>.
/// </summary>
/// <remarks>
/// <para>
/// Attributes are integers clamped to [-100, 100]. They can be owned by a character,
/// a directed relation between two characters, the current scene, or the whole world —
/// the owner is captured by <see cref="AttributeAddress"/>, not by the ID itself.
/// </para>
/// <para>
/// Use PascalCase names that read naturally in a game design context:
/// <c>"Trust"</c>, <c>"Fear"</c>, <c>"SuspicionLevel"</c>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var trust  = new AttributeId("Trust");
/// var tension = new AttributeId("Tension");
/// </code>
/// </example>
/// <param name="Value">The name of the attribute (e.g. <c>"Trust"</c>).</param>
public readonly record struct AttributeId(string Value);
