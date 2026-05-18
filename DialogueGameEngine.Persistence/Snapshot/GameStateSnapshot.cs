namespace DialogueGameEngine.Persistence.Snapshot;

using DialogueGameEngine.Core;

public sealed record GameStateSnapshot
{
    public required string CurrentScene { get; init; }
    public IReadOnlyList<AttributeSnapshot> Attributes { get; init; } = [];
    public IReadOnlyList<string> Flags { get; init; } = [];

    // Builds a snapshot from a live GameState
    public static GameStateSnapshot From(GameState state)
    {
        return new GameStateSnapshot
        {
            CurrentScene = state.CurrentScene.Value,
            Attributes = state.GetAllAttributes()
                .Select(kv => new AttributeSnapshot { Address = kv.Key, Value = kv.Value.Value })
                .ToList(),
            Flags = state.GetAllFlags().Select(f => f.Value).ToList()
        };
    }

    // Reconstructs a GameState from this snapshot
    public GameState ToGameState()
    {
        var state = new GameState(new SceneId(CurrentScene));
        foreach (var attr in Attributes)
            state.SetAttribute(attr.Address, attr.Value);
        foreach (var flag in Flags)
            state.SetFlag(new FlagId(flag));
        return state;
    }
}
