namespace DialogueGameEngine.Cli.Commands;

using DialogueGameEngine.Cli.Options;
using DialogueGameEngine.Cli.Runner;
using DialogueGameEngine.Core;
using DialogueGameEngine.Persistence;
using DialogueGameEngine.Persistence.Repositories;
using Spectre.Console;

internal sealed class PlayCommand : ICliCommand
{
    public int Execute(ParsedArguments args, CommandContext context)
    {
        const string usage = "dialogue-engine --story <dir> [--save <file>] [--start <scene-id>]";
        if (args.HasFlag("--help"))
        {
            Cli.Display.CliHelp.PrintGeneral();
            return 0;
        }

        var storyDir = CommandHelpers.RequireStoryPath(args, usage);
        if (storyDir is null)
            return 1;

        var saveFile = args.GetOption("--save") ?? CommandHelpers.GetDefaultSaveFile(storyDir);
        var startScene = args.GetOption("--start");

        var documents = CommandHelpers.LoadSceneDocuments(storyDir, context);
        var initialState = CommandHelpers.LoadInitialState(storyDir, context);
        if (!CommandHelpers.ValidateOrRenderErrors(documents, initialState, startScene))
            return 1;

        var scenes = documents.Select(d => d.Scene).ToArray();
        var options = new PersistenceOptions { StoryDirectory = storyDir, SaveFilePath = saveFile };
        var saveRepo = new JsonGameStateRepository(options, context.SerializerOptions);

        GameState state;
        var saved = saveRepo.Load(hasStartedDefault: true);
        if (saved is not null)
        {
            state = saved;
            AnsiConsole.MarkupLine($"[dim]Resuming from scene:[/] [yellow]{Markup.Escape(state.CurrentScene.Value)}[/]");
        }
        else
        {
            state = CommandHelpers.CreateStartingState(storyDir, startScene, scenes, initialState);
        }

        var engine = new DialogueEngine(documents);
        engine.Start(state);

        var runner = new GameRunner(engine, saveRepo, scenes);
        runner.Run(state);
        return 0;
    }
}

