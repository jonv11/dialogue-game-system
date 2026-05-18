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
//   dialogue-engine edit  --story <dir>
//   dialogue-engine reset --story <dir> [--save <file>]
//   dialogue-engine stats --story <dir> [--start <scene-id>]
//
// Subcommands
//   (default)  Play a story
//   edit       Open the interactive story editor
//   reset      Delete the save file for a story
//   stats      Print story graph statistics (scenes, paths, endings, …)
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

var isEdit  = args.Length > 0 && args[0] == "edit";
var isReset = args.Length > 0 && args[0] == "reset";
var isStats = args.Length > 0 && args[0] == "stats";
var argSpan = (isEdit || isReset || isStats) ? args[1..] : args;

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
    saveFile ??= Path.Combine(Path.GetDirectoryName(storyDir) ?? ".", "save.json");
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

    SceneDefinition[] statsScenes;
    try { statsScenes = statsSceneRepo.LoadAll().ToArray(); }
    catch (DirectoryNotFoundException ex)
    {
        AnsiConsole.MarkupLine($"[red]Error:[/] {Markup.Escape(ex.Message)}");
        return 1;
    }

    if (statsScenes.Length == 0)
    {
        AnsiConsole.MarkupLine($"[red]Error:[/] No scene files found in [yellow]{Markup.Escape(storyDir)}[/]");
        return 1;
    }

    SceneId statsStartId;
    if (startScene is not null)
    {
        statsStartId = new SceneId(startScene);
    }
    else
    {
        var initFile = Path.Combine(storyDir, "_initial-state.json");
        if (File.Exists(initFile))
        {
            var initOpts = new PersistenceOptions { StoryDirectory = storyDir, SaveFilePath = initFile };
            var initRepo = new JsonGameStateRepository(initOpts, serializerOptions);
            statsStartId = initRepo.Load()?.CurrentScene ?? statsScenes.OrderBy(s => s.Id.Value).First().Id;
        }
        else
        {
            statsStartId = statsScenes.OrderBy(s => s.Id.Value).First().Id;
        }
    }

    var analyzer = new StoryGraphAnalyzer(statsScenes);
    var statsResult = analyzer.Analyze(statsStartId);
    StoryStatsRenderer.Render(statsResult, statsStartId);
    return 0;
}

// ── Play mode ────────────────────────────────────────────────────────────────
saveFile ??= Path.Combine(Path.GetDirectoryName(storyDir) ?? ".", "save.json");

var options  = new PersistenceOptions { StoryDirectory = storyDir, SaveFilePath = saveFile };
var sceneRepo = new JsonSceneRepository(options, serializerOptions);
var saveRepo  = new JsonGameStateRepository(options, serializerOptions);

SceneDefinition[] scenes;
try
{
    scenes = sceneRepo.LoadAll().ToArray();
}
catch (DirectoryNotFoundException ex)
{
    AnsiConsole.MarkupLine($"[red]Error:[/] {Markup.Escape(ex.Message)}");
    return 1;
}

if (scenes.Length == 0)
{
    AnsiConsole.MarkupLine($"[red]Error:[/] No scene files found in [yellow]{Markup.Escape(storyDir)}[/]");
    return 1;
}

// Resolve starting state: saved progress → explicit --start → initial-state.json → first scene alphabetically
GameState state;
var saved = saveRepo.Load();
if (saved is not null)
{
    state = saved;
    AnsiConsole.MarkupLine($"[dim]Resuming from scene:[/] [yellow]{state.CurrentScene.Value}[/]");
}
else if (startScene is not null)
{
    state = new GameState(new SceneId(startScene));
}
else
{
    var initialStateFile = Path.Combine(storyDir, "_initial-state.json");
    if (File.Exists(initialStateFile))
    {
        var initialOptions = new PersistenceOptions { StoryDirectory = storyDir, SaveFilePath = initialStateFile };
        var initialRepo    = new JsonGameStateRepository(initialOptions, serializerOptions);
        state = initialRepo.Load() ?? new GameState(scenes.OrderBy(s => s.Id.Value).First().Id);
    }
    else
    {
        state = new GameState(scenes.OrderBy(s => s.Id.Value).First().Id);
    }
}

var engine = new DialogueEngine(scenes);
var runner = new GameRunner(engine, saveRepo);
runner.Run(state);
return 0;

static void PrintHelp()
{
    AnsiConsole.MarkupLine("[bold yellow]dialogue-engine[/] — A branching narrative dialogue engine");
    AnsiConsole.WriteLine();
    AnsiConsole.MarkupLine("[bold]USAGE[/]");
    AnsiConsole.MarkupLine("  dialogue-engine [bold]--story[/] [italic]<dir>[/] [[OPTIONS]]");
    AnsiConsole.MarkupLine("  dialogue-engine edit  [bold]--story[/] [italic]<dir>[/]");
    AnsiConsole.MarkupLine("  dialogue-engine reset [bold]--story[/] [italic]<dir>[/] [[[bold]--save[/] [italic]<file>[/]]]");
    AnsiConsole.MarkupLine("  dialogue-engine stats [bold]--story[/] [italic]<dir>[/] [[[bold]--start[/] [italic]<scene-id>[/]]]");
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
    AnsiConsole.MarkupLine("[bold]PLAYER CONTROLS[/]");
    AnsiConsole.MarkupLine("  [bold]1[/]–[bold]9[/]   Select a choice");
    AnsiConsole.MarkupLine("  [bold]s[/]     Save progress");
    AnsiConsole.MarkupLine("  [bold]q[/]     Save and quit");
}
