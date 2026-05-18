namespace DialogueGameEngine.Persistence.Repositories;

using System.Text.Json;
using DialogueGameEngine.Core;

public sealed class JsonSceneRepository : ISceneRepository
{
    private readonly string _directory;
    private readonly JsonSerializerOptions _options;

    public JsonSceneRepository(PersistenceOptions options, JsonSerializerOptions serializerOptions)
    {
        _directory = options.StoryDirectory;
        _options = serializerOptions;
    }

    public IEnumerable<SceneDefinition> LoadAll()
    {
        if (!Directory.Exists(_directory))
            throw new DirectoryNotFoundException($"Story directory not found: '{_directory}'");

        // Files starting with '_' are metadata (initial-state, config) — not scene definitions.
        // Scans recursively so scenes can be grouped into subdirectories (e.g. act1/, act2/).
        // The '_' exclusion applies to the filename only, not to directory names.
        foreach (var file in Directory.EnumerateFiles(_directory, "*.json", SearchOption.AllDirectories)
            .Where(f => !Path.GetFileName(f).StartsWith('_')))
        {
            SceneDefinition? scene = null;
            try
            {
                var json = File.ReadAllText(file);
                scene = JsonSerializer.Deserialize<SceneDefinition>(json, _options);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[Persistence] Skipping '{Path.GetFileName(file)}': {ex.Message}");
            }

            if (scene is not null)
                yield return scene;
        }
    }
}
