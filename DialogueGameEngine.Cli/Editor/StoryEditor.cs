namespace DialogueGameEngine.Cli.Editor;

using DialogueGameEngine.Core;
using DialogueGameEngine.Persistence;
using DialogueGameEngine.Persistence.Repositories;
using Spectre.Console;
using System.Text.Json;

internal sealed class StoryEditor
{
    private readonly string _storyDir;
    private readonly JsonSerializerOptions _options;
    private readonly Dictionary<string, SceneDefinition> _scenes;

    internal StoryEditor(string storyDir, JsonSerializerOptions options)
    {
        _storyDir = storyDir;
        _options  = options;
        _scenes   = new Dictionary<string, SceneDefinition>(StringComparer.OrdinalIgnoreCase);

        LoadScenes();
    }

    internal void Run()
    {
        while (true)
        {
            AnsiConsole.Clear();

            try { AnsiConsole.Write(new FigletText("Story Editor").Color(Color.Yellow)); } catch { /* non-interactive */ }

            AnsiConsole.MarkupLine($"[dim]{Markup.Escape(_storyDir)}[/]");
            AnsiConsole.MarkupLine($"[dim]{_scenes.Count} scene(s)[/]");
            AnsiConsole.WriteLine();

            var menuItems = new List<string> { "[+] New scene" };
            menuItems.AddRange(
                _scenes.Values
                    .OrderBy(s => s.Id.Value)
                    .Select(s => $"{s.Id.Value} — \"{s.Title}\" ({s.Choices.Count} choices)"));
            menuItems.Add("[Q] Quit");

            var selected = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select a scene to edit:")
                    .PageSize(20)
                    .AddChoices(menuItems));

            if (selected == "[Q] Quit")
                break;

            if (selected == "[+] New scene")
                CreateScene();
            else
                EditExistingScene(selected);
        }
    }

    private void CreateScene()
    {
        var id = AnsiConsole.Prompt(new TextPrompt<string>("Scene ID (snake_case):"));

        if (_scenes.ContainsKey(id))
        {
            AnsiConsole.MarkupLine($"[red]A scene with ID '{Markup.Escape(id)}' already exists.[/]");
            AnsiConsole.MarkupLine("[dim]Press Enter to continue.[/]");
            Console.ReadLine();
            return;
        }

        var title = AnsiConsole.Prompt(new TextPrompt<string>("Title:"));
        var scene = new SceneDefinition { Id = new SceneId(id), Title = title };
        _scenes[id] = scene;

        OpenSceneEditor(id, scene, dirty: true);
    }

    private void EditExistingScene(string menuItem)
    {
        // Format: "{sceneId} — \"{title}\" ({n} choices)"
        var sceneId = menuItem.Split(" — ")[0];

        if (_scenes.TryGetValue(sceneId, out var scene))
            OpenSceneEditor(sceneId, scene, dirty: false);
    }

    private void OpenSceneEditor(string sceneId, SceneDefinition scene, bool dirty)
    {
        var knownIds = _scenes.Keys.OrderBy(x => x).ToList();
        var menu     = new SceneEditorMenu(_storyDir, _options, scene, knownIds, dirty);
        var result   = menu.Run();

        if (result is null)
            _scenes.Remove(sceneId);
        else
            _scenes[sceneId] = result;
    }

    private void LoadScenes()
    {
        if (!Directory.Exists(_storyDir))
        {
            Directory.CreateDirectory(_storyDir);
            return;
        }

        var persistenceOptions = new PersistenceOptions
        {
            StoryDirectory = _storyDir,
            SaveFilePath   = string.Empty,
        };

        var repo = new JsonSceneRepository(persistenceOptions, _options);

        foreach (var document in repo.LoadAllDocuments())
        {
            var scene = document.Scene;
            if (_scenes.ContainsKey(scene.Id.Value))
            {
                throw new StoryValidationException(
                    $"Duplicate scene id '{scene.Id.Value}' found.",
                    [
                        StoryValidationIssue.Error(
                            StoryValidationCodes.DuplicateSceneId,
                            $"Duplicate scene id '{scene.Id.Value}' found while loading the editor.",
                            document.FilePath,
                            scene.Id.Value)
                    ]);
            }

            _scenes[scene.Id.Value] = scene;
        }
    }
}
