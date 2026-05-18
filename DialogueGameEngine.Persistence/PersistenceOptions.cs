namespace DialogueGameEngine.Persistence;

/// <summary>Configuration for where story and save files are located on disk.</summary>
public sealed record PersistenceOptions
{
    /// <summary>Directory that contains one .json file per scene definition.</summary>
    public required string StoryDirectory { get; init; }

    /// <summary>Path to the save file that stores current game state. Default: save.json next to story directory.</summary>
    public string SaveFilePath { get; init; } = "save.json";
}
