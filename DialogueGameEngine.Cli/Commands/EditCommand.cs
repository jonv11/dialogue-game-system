namespace DialogueGameEngine.Cli.Commands;

using DialogueGameEngine.Cli.Editor;
using DialogueGameEngine.Cli.Options;

internal sealed class EditCommand : ICliCommand
{
    public int Execute(ParsedArguments args, CommandContext context)
    {
        const string usage = "dialogue-engine edit --story <dir>";
        var storyDir = CommandHelpers.RequireStoryPath(args, usage);
        if (storyDir is null)
            return 1;

        var editor = new StoryEditor(storyDir, context.SerializerOptions);
        editor.Run();
        return 0;
    }
}

