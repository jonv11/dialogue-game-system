namespace DialogueGameEngine.Persistence.Repositories;

using System.Text.Json;
using DialogueGameEngine.Core;
using DialogueGameEngine.Persistence.Snapshot;

public sealed class JsonGameStateRepository : IGameStateRepository
{
    private readonly string _savePath;
    private readonly JsonSerializerOptions _options;

    public JsonGameStateRepository(PersistenceOptions options, JsonSerializerOptions serializerOptions)
    {
        _savePath = options.SaveFilePath;
        _options = serializerOptions;
    }

    public GameState? Load()
    {
        if (!File.Exists(_savePath))
            return null;

        var json = File.ReadAllText(_savePath);
        var snapshot = JsonSerializer.Deserialize<GameStateSnapshot>(json, _options);
        return snapshot?.ToGameState();
    }

    public void Save(GameState state)
    {
        var snapshot = GameStateSnapshot.From(state);
        var json = JsonSerializer.Serialize(snapshot, _options);
        File.WriteAllText(_savePath, json);
    }
}
