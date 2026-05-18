namespace DialogueGameEngine.Cli.Commands;

using DialogueGameEngine.Cli.Display;
using DialogueGameEngine.Cli.Options;
using DialogueGameEngine.Core.Analysis;

internal sealed class StatsCommand : ICliCommand
{
    public int Execute(ParsedArguments args, CommandContext context)
    {
        const string usage = "dialogue-engine stats --story <dir> [--start <scene-id>]";
        var storyDir = CommandHelpers.RequireStoryPath(args, usage);
        if (storyDir is null)
            return 1;

        var startScene = args.GetOption("--start");
        var documents = CommandHelpers.LoadSceneDocuments(storyDir, context);
        var initialState = CommandHelpers.LoadInitialState(storyDir, context);
        if (!CommandHelpers.ValidateOrRenderErrors(documents, initialState, startScene))
            return 1;

        var scenes = documents.Select(d => d.Scene).ToArray();
        var startState = CommandHelpers.CreateStartingState(storyDir, startScene, scenes, initialState);
        var analyzer = new StoryGraphAnalyzer(scenes);
        var stats = analyzer.Analyze(startState.CurrentScene);
        StoryStatsRenderer.Render(stats, startState.CurrentScene);
        return 0;
    }
}

