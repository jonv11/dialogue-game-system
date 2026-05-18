namespace DialogueGameEngine.Persistence.Repositories;

using System.Text.Json;
using DialogueGameEngine.Core;

public sealed class JsonInitialStateRepository
{
    private readonly string _initialStatePath;
    private readonly JsonSerializerOptions _options;

    public JsonInitialStateRepository(PersistenceOptions options, JsonSerializerOptions serializerOptions)
    {
        _initialStatePath = Path.Combine(options.StoryDirectory, "_initial-state.json");
        _options = serializerOptions;
    }

    public bool Exists => File.Exists(_initialStatePath);

    public InitialStateDefinition? Load()
    {
        if (!Exists)
            return null;

        try
        {
            var json = File.ReadAllText(_initialStatePath);
            var initialState = JsonSerializer.Deserialize<InitialStateDefinition>(json, _options);
            if (initialState is null)
                throw new JsonException("Initial state file deserialized to null.");

            return initialState;
        }
        catch (Exception ex)
        {
            throw new StoryValidationException(
                $"Failed to load initial state '{_initialStatePath}'.",
                [
                    StoryValidationIssue.Error(
                        StoryValidationCodes.InvalidInitialState,
                        $"Failed to load '{_initialStatePath}': {ex.Message}",
                        _initialStatePath)
                ]);
        }
    }
}

