namespace DialogueGameEngine.Persistence.Repositories;

using DialogueGameEngine.Core;

/// <summary>Loads scene definitions from persistent storage.</summary>
public interface ISceneRepository
{
    /// <summary>Loads all scene definitions. Each file in the story directory is one scene.</summary>
    IEnumerable<SceneDefinition> LoadAll();

    /// <summary>Loads all scene definitions with source file metadata.</summary>
    IReadOnlyList<SceneDocument> LoadAllDocuments();
}
