namespace DialogueGameEngine.Cli.Commands;

using DialogueGameEngine.Cli.Options;
using DialogueGameEngine.Cli.Runner;
using DialogueGameEngine.Core;
using DialogueGameEngine.Persistence;
using DialogueGameEngine.Persistence.Repositories;

internal sealed class InspectCommand : ICliCommand
{
    public int Execute(ParsedArguments args, CommandContext context)
    {
        const string usage = "dialogue-engine inspect --story <dir> [--save <file>] [--start <scene-id>]";
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
        var state = saveRepo.Load(hasStartedDefault: true)
            ?? CommandHelpers.CreateStartingState(storyDir, startScene, scenes, initialState);

        var engine = new DialogueEngine(documents);
        engine.Start(state);
        InspectRunner.Run(engine, state, scenes);
        return 0;
    }
}

