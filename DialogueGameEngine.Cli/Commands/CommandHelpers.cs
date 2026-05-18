namespace DialogueGameEngine.Cli.Commands;

using DialogueGameEngine.Cli.Display;
using DialogueGameEngine.Cli.Options;
using DialogueGameEngine.Core;
using DialogueGameEngine.Persistence;
using DialogueGameEngine.Persistence.Repositories;
using Spectre.Console;

internal static class CommandHelpers
{
    internal static string? RequireStoryPath(ParsedArguments args, string usage)
    {
        var storyDir = args.GetOption("--story");
        if (!string.IsNullOrWhiteSpace(storyDir))
            return Path.GetFullPath(storyDir);

        CliHelp.PrintUsage(usage);
        return null;
    }

    internal static string GetDefaultSaveFile(string storyDir) =>
        Path.Combine(Path.GetDirectoryName(storyDir) ?? ".", "save.json");

    internal static IReadOnlyList<SceneDocument> LoadSceneDocuments(
        string storyDir,
        CommandContext context,
        bool allowEmpty = false)
    {
        var persistenceOptions = new PersistenceOptions
        {
            StoryDirectory = storyDir,
            SaveFilePath = string.Empty
        };
        var repository = new JsonSceneRepository(persistenceOptions, context.SerializerOptions);
        var documents = repository.LoadAllDocuments();

        if (documents.Count == 0 && !allowEmpty)
        {
            throw new StoryValidationException(
                $"No scene files found in '{storyDir}'.",
                [
                    StoryValidationIssue.Error(
                        StoryValidationCodes.MissingStartScene,
                        $"No scene files found in '{storyDir}'.",
                        storyDir)
                ]);
        }

        return documents;
    }

    internal static InitialStateDefinition? LoadInitialState(string storyDir, CommandContext context)
    {
        var persistenceOptions = new PersistenceOptions
        {
            StoryDirectory = storyDir,
            SaveFilePath = string.Empty
        };
        return new JsonInitialStateRepository(persistenceOptions, context.SerializerOptions).Load();
    }

    internal static StoryValidationResult ValidateStory(
        IReadOnlyList<SceneDocument> documents,
        InitialStateDefinition? initialState,
        string? startScene,
        bool warningsAsErrors = false)
    {
        var options = new StoryValidationOptions
        {
            StartScene = string.IsNullOrWhiteSpace(startScene) ? null : new SceneId(startScene),
            WarningsAsErrors = warningsAsErrors
        };
        return new StoryValidator().Validate(documents, initialState, options);
    }

    internal static bool HasFailingIssues(StoryValidationResult result, bool warningsAsErrors) =>
        result.HasErrors || (warningsAsErrors && result.HasWarnings);

    internal static SceneId GetFallbackStartScene(IEnumerable<SceneDefinition> scenes) =>
        scenes.OrderBy(s => s.Id.Value).First().Id;

    internal static GameState CreateStartingState(
        string storyDir,
        string? startScene,
        IReadOnlyCollection<SceneDefinition> scenes,
        InitialStateDefinition? initialState)
    {
        if (!string.IsNullOrWhiteSpace(startScene))
            return new GameState(new SceneId(startScene));

        var fallback = GetFallbackStartScene(scenes);
        return initialState?.ToGameState(fallback, hasStarted: false) ?? new GameState(fallback);
    }

    internal static bool ValidateOrRenderErrors(
        IReadOnlyList<SceneDocument> documents,
        InitialStateDefinition? initialState,
        string? startScene)
    {
        var validation = ValidateStory(documents, initialState, startScene);
        if (!validation.HasErrors)
            return true;

        ValidationResultRenderer.RenderText(validation);
        return false;
    }

    internal static void RenderSaveDeleted(string saveFile)
    {
        AnsiConsole.MarkupLine($"[green]Save deleted:[/] [yellow]{Markup.Escape(saveFile)}[/]");
    }
}
