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
        foreach (var document in LoadAllDocuments())
            yield return document.Scene;
    }

    public IReadOnlyList<SceneDocument> LoadAllDocuments()
    {
        if (!Directory.Exists(_directory))
            throw new StoryValidationException(
                $"Story directory not found: '{_directory}'",
                [
                    StoryValidationIssue.Error(
                        StoryValidationCodes.StoryPathNotFound,
                        $"Story directory not found: '{_directory}'",
                        _directory)
                ]);

        var documents = new List<SceneDocument>();
        // Files starting with '_' are metadata (initial-state, config) — not scene definitions.
        // Scans recursively so scenes can be grouped into subdirectories (e.g. act1/, act2/).
        // The '_' exclusion applies to the filename only, not to directory names.
        foreach (var file in Directory.EnumerateFiles(_directory, "*.json", SearchOption.AllDirectories)
            .Where(f => !Path.GetFileName(f).StartsWith('_'))
            .OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
        {
            try
            {
                var json = File.ReadAllText(file);
                var scene = JsonSerializer.Deserialize<SceneDefinition>(json, _options);
                if (scene is null)
                    throw new JsonException("Scene file deserialized to null.");

                documents.Add(new SceneDocument { Scene = scene, FilePath = file });
            }
            catch (Exception ex)
            {
                throw new StoryValidationException(
                    $"Failed to load scene file '{file}'.",
                    [CreateLoadIssue(file, ex)]);
            }
        }

        return documents;
    }

    private static StoryValidationIssue CreateLoadIssue(string file, Exception exception)
    {
        var message = exception.Message;
        var code = StoryValidationCodes.InvalidJson;

        if (message.Contains("missing required", StringComparison.OrdinalIgnoreCase))
            code = StoryValidationCodes.MissingRequiredField;
        else if (message.Contains("Unknown condition type", StringComparison.OrdinalIgnoreCase) ||
                 message.Contains("discriminator on condition", StringComparison.OrdinalIgnoreCase))
            code = StoryValidationCodes.UnknownConditionType;
        else if (message.Contains("Unknown effect type", StringComparison.OrdinalIgnoreCase) ||
                 message.Contains("discriminator on effect", StringComparison.OrdinalIgnoreCase))
            code = StoryValidationCodes.UnknownEffectType;

        return StoryValidationIssue.Error(
            code,
            $"Failed to load '{file}': {message}",
            file);
    }
}
