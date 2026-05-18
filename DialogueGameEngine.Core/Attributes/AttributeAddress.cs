namespace DialogueGameEngine.Core;

/// <summary>
/// Points to a specific attribute value anywhere in the system — on a character, a relation,
/// a scene, or the world.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="AttributeAddress"/> is the engine's universal pointer for attribute reads and writes.
/// Conditions, effects, and modifiers all reference attributes through this type,
/// so they don't need to know what owns the attribute.
/// </para>
/// <para>
/// Always create instances via the static factory methods rather than constructing directly.
/// Unused optional fields (e.g. <see cref="Character"/> for a <see cref="AttributeScope.World"/> address) are left null.
/// </para>
/// <para>
/// Because this is a <c>sealed record</c>, structural equality is generated automatically —
/// two addresses with the same scope and field values compare as equal and hash identically,
/// making them safe to use as <see cref="System.Collections.Generic.Dictionary{TKey,TValue}"/> keys.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var playerTrust  = AttributeAddress.CharacterAttribute(new CharacterId("Player"), new AttributeId("Trust"));
/// var miraTrust    = AttributeAddress.RelationAttribute(new CharacterId("Mira"), new CharacterId("Player"), new AttributeId("Trust"));
/// var sceneTension = AttributeAddress.SceneAttribute(new SceneId("kitchen_argument"), new AttributeId("Tension"));
/// var worldPressure = AttributeAddress.WorldAttribute(new AttributeId("AuthorityPressure"));
/// </code>
/// </example>
public sealed record AttributeAddress
{
    /// <summary>Which domain this attribute belongs to.</summary>
    public required AttributeScope Scope { get; init; }

    /// <summary>The named attribute within the owner's domain (e.g. <c>"Trust"</c>).</summary>
    public required AttributeId Attribute { get; init; }

    /// <summary>The owning character. Set only when <see cref="Scope"/> is <see cref="AttributeScope.Character"/>.</summary>
    public CharacterId? Character { get; init; }

    /// <summary>The source character of a directed relation. Set only when <see cref="Scope"/> is <see cref="AttributeScope.Relation"/>.</summary>
    public CharacterId? From { get; init; }

    /// <summary>The target character of a directed relation. Set only when <see cref="Scope"/> is <see cref="AttributeScope.Relation"/>.</summary>
    public CharacterId? To { get; init; }

    /// <summary>The owning scene. Set only when <see cref="Scope"/> is <see cref="AttributeScope.Scene"/>.</summary>
    public SceneId? Scene { get; init; }

    /// <summary>Creates an address for an attribute owned by a single character.</summary>
    /// <param name="character">The character who owns this attribute.</param>
    /// <param name="attribute">The attribute name (e.g. <c>"Openness"</c>).</param>
    public static AttributeAddress CharacterAttribute(CharacterId character, AttributeId attribute) =>
        new() { Scope = AttributeScope.Character, Character = character, Attribute = attribute };

    /// <summary>
    /// Creates an address for an attribute on a directed relation between two characters.
    /// </summary>
    /// <param name="from">The character whose perspective this relation is measured from.</param>
    /// <param name="to">The character being observed or related to.</param>
    /// <param name="attribute">The attribute name (e.g. <c>"Trust"</c>).</param>
    public static AttributeAddress RelationAttribute(CharacterId from, CharacterId to, AttributeId attribute) =>
        new() { Scope = AttributeScope.Relation, From = from, To = to, Attribute = attribute };

    /// <summary>Creates an address for a scene-level attribute such as ambient tension.</summary>
    /// <param name="scene">The scene that owns this attribute.</param>
    /// <param name="attribute">The attribute name (e.g. <c>"Tension"</c>).</param>
    public static AttributeAddress SceneAttribute(SceneId scene, AttributeId attribute) =>
        new() { Scope = AttributeScope.Scene, Scene = scene, Attribute = attribute };

    /// <summary>Creates an address for a world-level attribute shared across all scenes.</summary>
    /// <param name="attribute">The attribute name (e.g. <c>"AuthorityPressure"</c>).</param>
    public static AttributeAddress WorldAttribute(AttributeId attribute) =>
        new() { Scope = AttributeScope.World, Attribute = attribute };
}
