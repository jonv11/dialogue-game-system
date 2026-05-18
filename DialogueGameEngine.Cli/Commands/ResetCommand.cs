namespace DialogueGameEngine.Cli.Commands;

using DialogueGameEngine.Cli.Options;
using DialogueGameEngine.Persistence;
using DialogueGameEngine.Persistence.Repositories;
using Spectre.Console;

internal sealed class ResetCommand : ICliCommand
{
    public int Execute(ParsedArguments args, CommandContext context)
    {
        const string usage = "dialogue-engine reset --story <dir> [--save <file>]";
        var storyDir = CommandHelpers.RequireStoryPath(args, usage);
        if (storyDir is null)
            return 1;

        var saveFile = args.GetOption("--save") ?? CommandHelpers.GetDefaultSaveFile(storyDir);
        var options = new PersistenceOptions { StoryDirectory = storyDir, SaveFilePath = saveFile };
        var saveRepo = new JsonGameStateRepository(options, context.SerializerOptions);

        if (saveRepo.Delete())
            CommandHelpers.RenderSaveDeleted(saveFile);
        else
            AnsiConsole.MarkupLine($"[dim]No save file found at:[/] [yellow]{Markup.Escape(saveFile)}[/]");

        return 0;
    }
}

