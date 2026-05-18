namespace DialogueGameEngine.Cli.Commands;

using DialogueGameEngine.Cli.Options;

internal interface ICliCommand
{
    int Execute(ParsedArguments args, CommandContext context);
}

