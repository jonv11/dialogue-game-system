namespace DialogueGameEngine.Core;

/// <summary>Raw initial story state as loaded from _initial-state.json.</summary>
public sealed record InitialStateDefinition
{
    public string? CurrentScene { get; init; }
    public IReadOnlyList<InitialAttributeDefinition> Attributes { get; init; } = [];
    public IReadOnlyList<string?> Flags { get; init; } = [];

    public GameState ToGameState(SceneId fallbackScene, bool hasStarted = false)
    {
        var scene = string.IsNullOrWhiteSpace(CurrentScene)
            ? fallbackScene
            : new SceneId(CurrentScene);

        var state = new GameState(scene, hasStarted);

        foreach (var attribute in Attributes)
        {
            if (attribute.Address is not null && attribute.Value is not null)
                state.SetAttribute(attribute.Address, attribute.Value.Value);
        }

        foreach (var flag in Flags)
        {
            if (!string.IsNullOrWhiteSpace(flag))
                state.SetFlag(new FlagId(flag));
        }

        return state;
    }
}

public sealed record InitialAttributeDefinition
{
    public AttributeAddress? Address { get; init; }
    public int? Value { get; init; }
}

