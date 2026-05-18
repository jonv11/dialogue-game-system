namespace DialogueGameEngine.Cli;

using DialogueGameEngine.Cli.Commands;
using DialogueGameEngine.Cli.Display;
using DialogueGameEngine.Cli.Options;
using DialogueGameEngine.Core;
using DialogueGameEngine.Persistence;
using Spectre.Console;

public static class CliApplication
{
    public static int Run(string[] args)
    {
        var commands = new Dictionary<string, ICliCommand>(StringComparer.OrdinalIgnoreCase)
        {
            ["play"] = new PlayCommand(),
            ["edit"] = new EditCommand(),
            ["reset"] = new ResetCommand(),
            ["stats"] = new StatsCommand(),
            ["inspect"] = new InspectCommand(),
            ["validate"] = new ValidateCommand(),
        };

        var parseResult = ArgumentParser.Parse(args, commands.Keys);
        if (parseResult.ShowHelp)
        {
            CliHelp.PrintGeneral();
            return 0;
        }

        if (parseResult.ShowVersion)
        {
            var version = typeof(CliApplication).Assembly.GetName().Version?.ToString(3) ?? "1.0.0";
            AnsiConsole.MarkupLine($"dialogue-engine {version}");
            return 0;
        }

        if (parseResult.ErrorMessage is not null)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] {Markup.Escape(parseResult.ErrorMessage)}");
            CliHelp.PrintGeneral();
            return 1;
        }

        if (!commands.TryGetValue(parseResult.Arguments.CommandName, out var command))
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] Unknown command '{Markup.Escape(parseResult.Arguments.CommandName)}'.");
            CliHelp.PrintGeneral();
            return 1;
        }

        try
        {
            var context = new CommandContext(SerializerOptionsFactory.Create());
            return command.Execute(parseResult.Arguments, context);
        }
        catch (StoryValidationException ex)
        {
            var result = new StoryValidationResult();
            result.AddRange(ex.Issues);
            ValidationResultRenderer.RenderText(result);
            return 1;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] {Markup.Escape(ex.Message)}");
            return 1;
        }
    }
}

