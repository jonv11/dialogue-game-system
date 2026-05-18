using System.Text.Json;
using DialogueGameEngine.Cli.Display;
using DialogueGameEngine.Cli.Editor;
using DialogueGameEngine.Cli.Runner;
using DialogueGameEngine.Core;
using DialogueGameEngine.Core.Analysis;
using DialogueGameEngine.Persistence;
using DialogueGameEngine.Persistence.Repositories;
using Spectre.Console;

// ──────────────────────────────────────────────────────────────────────────────
// Usage:
//   dialogue-engine --story <dir> [--save <file>] [--start <scene-id>]
//   dialogue-engine edit    --story <dir>
//   dialogue-engine reset   --story <dir> [--save <file>]
//   dialogue-engine stats   --story <dir> [--start <scene-id>]
//   dialogue-engine inspect --story <dir> [--save <file>] [--start <scene-id>]
//
// Subcommands
//   (default)  Play a story
//   edit       Open the interactive story editor
//   reset      Delete the save file for a story
//   stats      Print story graph statistics (scenes, paths, endings, …)
//   inspect    Print a full debug snapshot of current game state
//
// --story   Directory containing .json scene files (required)
// --save    Path to the save file (default: save.json next to --story)
// --start   Scene ID to start from when no save file exists
// ──────────────────────────────────────────────────────────────────────────────

if (args.Contains("--help") || args.Contains("-h"))
{
    PrintHelp();
    return 0;
}

if (args.Contains("--version") || args.Contains("-v"))
{
    var ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.0.0";
    AnsiConsole.MarkupLine($"dialogue-engine {ver}");
    return 0;
}

var isEdit    = args.Length > 0 && args[0] == "edit";
var isReset   = args.Length > 0 && args[0] == "reset";
var isStats   = args.Length > 0 && args[0] == "stats";
var isInspect = args.Length > 0 && args[0] == "inspect";
var argSpan = (isEdit || isReset || isStats || isInspect) ? args[1..] : args;

string? storyDir   = null;
string? saveFile   = null;
string? startScene = null;

for (var i = 0; i < argSpan.Length; i++)
{
    switch (argSpan[i])
    {
        case "--story" when i + 1 < argSpan.Length:
            storyDir = argSpan[++i];
            break;
        case "--save" when i + 1 < argSpan.Length:
            saveFile = argSpan[++i];
            break;
        case "--start" when i + 1 < argSpan.Length:
            startScene = argSpan[++i];
            break;
    }
}

if (storyDir is null)
{
    if (isEdit)
        AnsiConsole.MarkupLine("[red]Usage:[/] dialogue-engine edit [bold]--story <dir>[/]");
    else if (isReset)
        AnsiConsole.MarkupLine("[red]Usage:[/] dialogue-engine reset [bold]--story <dir>[/] [[--save <file>]]");
    else if (isStats)
        AnsiConsole.MarkupLine("[red]Usage:[/] dialogue-engine stats [bold]--story <dir>[/] [[[bold]--start[/] [italic]<scene-id>[/]]]");
    else if (isInspect)
        AnsiConsole.MarkupLine("[red]Usage:[/] dialogue-engine inspect [bold]--story <dir>[/] [[[bold]--save[/] [italic]<file>[/]]] [[[bold]--start[/] [italic]<scene-id>[/]]]");
    else
        AnsiConsole.MarkupLine("[red]Usage:[/] dialogue-engine [bold]--story <dir>[/] [--save <file>] [--start <scene-id>]");
    return 1;
}

storyDir = Path.GetFullPath(storyDir);

var serializerOptions = SerializerOptionsFactory.Create();

// ── Editor mode ──────────────────────────────────────────────────────────────
if (isEdit)
{
    var editor = new StoryEditor(storyDir, serializerOptions);
    editor.Run();
    return 0;
}

// ── Reset mode ───────────────────────────────────────────────────────────────
if (isReset)
{
    saveFile ??= GetDefaultSaveFile(storyDir);
    var resetOptions  = new PersistenceOptions { StoryDirectory = storyDir, SaveFilePath = saveFile };
    var resetSaveRepo = new JsonGameStateRepository(resetOptions, serializerOptions);

    if (resetSaveRepo.Delete())
        AnsiConsole.MarkupLine($"[green]Save deleted:[/] [yellow]{Markup.Escape(saveFile)}[/]");
    else
        AnsiConsole.MarkupLine($"[dim]No save file found at:[/] [yellow]{Markup.Escape(saveFile)}[/]");

    return 0;
}

// ── Stats mode ───────────────────────────────────────────────────────────────
if (isStats)
{
    var statsOptions  = new PersistenceOptions { StoryDirectory = storyDir, SaveFilePath = string.Empty };
    var statsSceneRepo = new JsonSceneRepository(statsOptions, serializerOptions);

    if (!TryLoadScenes(statsSceneRepo, storyDir, out var statsScenes))
        return 1;

    var statsStartId = CreateStartingState(storyDir, startScene, statsScenes, serializerOptions).CurrentScene;

    var analyzer = new StoryGraphAnalyzer(statsScenes);
    var statsResult = analyzer.Analyze(statsStartId);
    StoryStatsRenderer.Render(statsResult, statsStartId);
    return 0;
}

// ── Inspect mode ─────────────────────────────────────────────────────────────
if (isInspect)
{
    saveFile ??= GetDefaultSaveFile(storyDir);

    var inspectOptions   = new PersistenceOptions { StoryDirectory = storyDir, SaveFilePath = saveFile };
    var inspectSceneRepo = new JsonSceneRepository(inspectOptions, serializerOptions);
    var inspectSaveRepo  = new JsonGameStateRepository(inspectOptions, serializerOptions);

    if (!TryLoadScenes(inspectSceneRepo, storyDir, out var inspectScenes))
        return 1;

    var inspectState = inspectSaveRepo.Load() ?? CreateStartingState(storyDir, startScene, inspectScenes, serializerOptions);
    var inspectEngine = new DialogueEngine(inspectScenes);
    InspectRunner.Run(inspectEngine, inspectState, inspectScenes);
    return 0;
}

// ── Play mode ────────────────────────────────────────────────────────────────
saveFile ??= GetDefaultSaveFile(storyDir);

var options  = new PersistenceOptions { StoryDirectory = storyDir, SaveFilePath = saveFile };
var sceneRepo = new JsonSceneRepository(options, serializerOptions);
var saveRepo  = new JsonGameStateRepository(options, serializerOptions);

if (!TryLoadScenes(sceneRepo, storyDir, out var scenes))
    return 1;

// Resolve starting state: saved progress → explicit --start → initial-state.json → first scene alphabetically
GameState state;
var saved = saveRepo.Load();
if (saved is not null)
{
    state = saved;
    AnsiConsole.MarkupLine($"[dim]Resuming from scene:[/] [yellow]{state.CurrentScene.Value}[/]");
}
else
{
    state = CreateStartingState(storyDir, startScene, scenes, serializerOptions);
}

var engine = new DialogueEngine(scenes);
var runner = new GameRunner(engine, saveRepo, scenes);
runner.Run(state);
return 0;

static string GetDefaultSaveFile(string storyDir) =>
    Path.Combine(Path.GetDirectoryName(storyDir) ?? ".", "save.json");

static bool TryLoadScenes(ISceneRepository sceneRepo, string storyDir, out SceneDefinition[] scenes)
{
    try
    {
        scenes = sceneRepo.LoadAll().ToArray();
    }
    catch (DirectoryNotFoundException ex)
    {
        scenes = [];
        AnsiConsole.MarkupLine($"[red]Error:[/] {Markup.Escape(ex.Message)}");
        return false;
    }

    if (scenes.Length > 0)
        return true;

    AnsiConsole.MarkupLine($"[red]Error:[/] No scene files found in [yellow]{Markup.Escape(storyDir)}[/]");
    return false;
}

static GameState CreateStartingState(
    string storyDir,
    string? startScene,
    IReadOnlyCollection<SceneDefinition> scenes,
    JsonSerializerOptions serializerOptions)
{
    if (startScene is not null)
        return new GameState(new SceneId(startScene));

    var initialStateFile = Path.Combine(storyDir, "_initial-state.json");
    if (!File.Exists(initialStateFile))
        return new GameState(GetFallbackStartScene(scenes));

    var initialOptions = new PersistenceOptions { StoryDirectory = storyDir, SaveFilePath = initialStateFile };
    var initialRepo    = new JsonGameStateRepository(initialOptions, serializerOptions);
    return initialRepo.Load() ?? new GameState(GetFallbackStartScene(scenes));
}

static SceneId GetFallbackStartScene(IEnumerable<SceneDefinition> scenes) =>
    scenes.OrderBy(s => s.Id.Value).First().Id;

static void PrintHelp()
{
    AnsiConsole.MarkupLine("[bold yellow]dialogue-engine[/] — A branching narrative dialogue engine");
    AnsiConsole.WriteLine();
    AnsiConsole.MarkupLine("[bold]USAGE[/]");
    AnsiConsole.MarkupLine("  dialogue-engine [bold]--story[/] [italic]<dir>[/] [[OPTIONS]]");
    AnsiConsole.MarkupLine("  dialogue-engine edit    [bold]--story[/] [italic]<dir>[/]");
    AnsiConsole.MarkupLine("  dialogue-engine reset   [bold]--story[/] [italic]<dir>[/] [[[bold]--save[/] [italic]<file>[/]]]");
    AnsiConsole.MarkupLine("  dialogue-engine stats   [bold]--story[/] [italic]<dir>[/] [[[bold]--start[/] [italic]<scene-id>[/]]]");
    AnsiConsole.MarkupLine("  dialogue-engine inspect [bold]--story[/] [italic]<dir>[/] [[[bold]--save[/] [italic]<file>[/]]] [[[bold]--start[/] [italic]<scene-id>[/]]]");
    AnsiConsole.MarkupLine("  dialogue-engine ([bold]--help[/] | [bold]-h[/])");
    AnsiConsole.MarkupLine("  dialogue-engine ([bold]--version[/] | [bold]-v[/])");
    AnsiConsole.WriteLine();
    AnsiConsole.MarkupLine("[bold]PLAY MODE OPTIONS[/]");
    AnsiConsole.MarkupLine("  [bold]--story[/] [italic]<dir>[/]       Directory containing .json scene files [dim][[required]][/]");
    AnsiConsole.MarkupLine("  [bold]--save[/]  [italic]<file>[/]      Save file path [dim][[default: save.json next to --story]][/]");
    AnsiConsole.MarkupLine("  [bold]--start[/] [italic]<scene-id>[/]  Starting scene when no save file exists");
    AnsiConsole.WriteLine();
    AnsiConsole.MarkupLine("[bold]EDIT MODE[/]");
    AnsiConsole.MarkupLine("  Opens the interactive story editor for the given story directory.");
    AnsiConsole.MarkupLine("  [bold]--story[/] [italic]<dir>[/]       Story directory to open (created if absent)");
    AnsiConsole.WriteLine();
    AnsiConsole.MarkupLine("[bold]RESET MODE[/]");
    AnsiConsole.MarkupLine("  Deletes the save file so the story can be played from the beginning.");
    AnsiConsole.MarkupLine("  [bold]--story[/] [italic]<dir>[/]       Story directory [dim][[required]][/]");
    AnsiConsole.MarkupLine("  [bold]--save[/]  [italic]<file>[/]      Save file to delete [dim][[default: save.json next to --story]][/]");
    AnsiConsole.WriteLine();
    AnsiConsole.MarkupLine("[bold]STATS MODE[/]");
    AnsiConsole.MarkupLine("  Prints story graph statistics: scenes, choices, endings, paths, orphans.");
    AnsiConsole.MarkupLine("  [bold]--story[/] [italic]<dir>[/]       Story directory [dim][[required]][/]");
    AnsiConsole.MarkupLine("  [bold]--start[/] [italic]<scene-id>[/]  Override starting scene for reachability analysis");
    AnsiConsole.WriteLine();
    AnsiConsole.MarkupLine("[bold]INSPECT MODE[/]");
    AnsiConsole.MarkupLine("  Prints a full debug snapshot: current scene, flags, attributes, active modifiers,");
    AnsiConsole.MarkupLine("  and all choices with their conditions and availability.");
    AnsiConsole.MarkupLine("  [bold]--story[/] [italic]<dir>[/]       Story directory [dim][[required]][/]");
    AnsiConsole.MarkupLine("  [bold]--save[/]  [italic]<file>[/]      Save file to inspect [dim][[default: save.json next to --story]][/]");
    AnsiConsole.MarkupLine("  [bold]--start[/] [italic]<scene-id>[/]  Override starting scene when no save file exists");
    AnsiConsole.WriteLine();
    AnsiConsole.MarkupLine("[bold]PLAYER CONTROLS[/]");
    AnsiConsole.MarkupLine("  [bold]1[/]–[bold]9[/]   Select a choice");
    AnsiConsole.MarkupLine("  [bold]s[/]     Save progress");
    AnsiConsole.MarkupLine("  [bold]d[/]     Print debug inspect (state, attributes, flags, choices)");
    AnsiConsole.MarkupLine("  [bold]q[/]     Save and quit");
}
