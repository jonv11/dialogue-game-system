namespace DialogueGameEngine.Persistence.Repositories;

using DialogueGameEngine.Core;

/// <summary>Saves and loads game state (progress, attributes, flags).</summary>
public interface IGameStateRepository
{
    /// <summary>Loads saved game state. Returns null if no save file exists.</summary>
    GameState? Load();

    /// <summary>Saves the current game state to disk.</summary>
    void Save(GameState state);
}
