namespace DialogueGameEngine.Core;

/// <summary>
/// Holds all mutable runtime facts about the current play session.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="GameState"/> is intentionally thin — it stores data and nothing else.
/// All evaluation, rules, and logic live in <see cref="DialogueEngine"/>, conditions, and effects.
/// </para>
/// <para>
/// <b>What lives here:</b>
/// <list type="bullet">
///   <item>Numeric attribute values for characters, relations, scenes, and the world.</item>
///   <item>Boolean flags that record which story events have occurred.</item>
///   <item>Which scene the player is currently in.</item>
/// </list>
/// </para>
/// <para>
/// To save and restore a session, use a persistence layer (such as
/// <c>JsonGameStateRepository</c>) that reads <see cref="GetAllAttributes"/> and
/// <see cref="GetAllFlags"/> and reconstructs state via the public mutation methods.
/// </para>
/// </remarks>
public sealed class GameState
{
    private readonly Dictionary<AttributeAddress, AttributeValue> _attributes = new();
    private readonly HashSet<FlagId> _flags = new();

    /// <summary>The scene the player is currently in.</summary>
    public SceneId CurrentScene { get; private set; }

    /// <summary>
    /// Creates a new game state, positioned at <paramref name="initialScene"/> with no attributes or flags set.
    /// </summary>
    /// <param name="initialScene">The scene where the story begins.</param>
    public GameState(SceneId initialScene)
    {
        CurrentScene = initialScene;
    }

    /// <summary>
    /// Returns the stored value of <paramref name="address"/>, or 0 if it has never been set.
    /// </summary>
    /// <param name="address">The attribute to read.</param>
    public AttributeValue GetAttribute(AttributeAddress address) =>
        _attributes.TryGetValue(address, out var value) ? value : new AttributeValue(0);

    /// <summary>
    /// Sets <paramref name="address"/> to <paramref name="value"/>, clamped to [-100, 100].
    /// Replaces any previously stored value.
    /// </summary>
    public void SetAttribute(AttributeAddress address, int value) =>
        _attributes[address] = new AttributeValue(value);

    /// <summary>
    /// Adds <paramref name="delta"/> to the current value of <paramref name="address"/>,
    /// clamping the result to [-100, 100].
    /// </summary>
    public void AddToAttribute(AttributeAddress address, int delta)
    {
        var current = GetAttribute(address);
        _attributes[address] = current.Add(delta);
    }

    /// <summary>Returns <see langword="true"/> if <paramref name="flag"/> is currently set.</summary>
    public bool HasFlag(FlagId flag) => _flags.Contains(flag);

    /// <summary>Marks <paramref name="flag"/> as having occurred. Safe to call multiple times.</summary>
    public void SetFlag(FlagId flag) => _flags.Add(flag);

    /// <summary>
    /// Removes <paramref name="flag"/>. If the flag was not set, this is a silent no-op.
    /// </summary>
    public void ClearFlag(FlagId flag) => _flags.Remove(flag);

    /// <summary>Moves the current scene pointer to <paramref name="scene"/>.</summary>
    public void MoveTo(SceneId scene) => CurrentScene = scene;

    /// <summary>
    /// Exposes all stored attribute values for serialization.
    /// Do not mutate the returned pairs — use <see cref="SetAttribute"/> instead.
    /// </summary>
    public IEnumerable<KeyValuePair<AttributeAddress, AttributeValue>> GetAllAttributes() => _attributes;

    /// <summary>
    /// Exposes all active flags for serialization.
    /// Do not mutate the returned set — use <see cref="SetFlag"/> and <see cref="ClearFlag"/> instead.
    /// </summary>
    public IEnumerable<FlagId> GetAllFlags() => _flags;
}
