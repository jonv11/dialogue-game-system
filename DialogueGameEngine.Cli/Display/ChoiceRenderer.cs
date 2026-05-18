namespace DialogueGameEngine.Cli.Display;

using DialogueGameEngine.Core;
using Spectre.Console;

/// <summary>Renders numbered player choices to the terminal.</summary>
internal static class ChoiceRenderer
{
    internal static void Render(IReadOnlyList<ChoiceDefinition> choices)
    {
        if (choices.Count == 0)
        {
            AnsiConsole.MarkupLine("[dim italic]  (No choices available — end of branch)[/]");
            return;
        }

        AnsiConsole.MarkupLine("[bold]What do you do?[/]");
        AnsiConsole.WriteLine();

        for (var i = 0; i < choices.Count; i++)
        {
            var choice = choices[i];
            AnsiConsole.MarkupLine(
                $"  [bold green]{i + 1}[/]. {Markup.Escape(choice.Text)}");
        }

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[dim]Enter a number, [bold]s[/] to save, [bold]d[/] to inspect, or [bold]q[/] to quit.[/]");
        AnsiConsole.WriteLine();
    }
}
