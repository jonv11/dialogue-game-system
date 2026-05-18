namespace DialogueGameEngine.Cli.Display;

using Spectre.Console;

internal static class CliHelp
{
    internal static void PrintGeneral()
    {
        AnsiConsole.MarkupLine("[bold yellow]dialogue-engine[/] — A branching narrative dialogue engine");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold]USAGE[/]");
        AnsiConsole.WriteLine("  dialogue-engine --story <dir> [OPTIONS]");
        AnsiConsole.WriteLine("  dialogue-engine edit     --story <dir>");
        AnsiConsole.WriteLine("  dialogue-engine reset    --story <dir> [--save <file>]");
        AnsiConsole.WriteLine("  dialogue-engine stats    --story <dir> [--start <scene-id>]");
        AnsiConsole.WriteLine("  dialogue-engine inspect  --story <dir> [--save <file>] [--start <scene-id>]");
        AnsiConsole.WriteLine("  dialogue-engine validate --story <dir> [--start <scene-id>] [--format text|json] [--warnings-as-errors]");
        AnsiConsole.WriteLine("  dialogue-engine (--help | -h)");
        AnsiConsole.WriteLine("  dialogue-engine (--version | -v)");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold]PLAY MODE OPTIONS[/]");
        AnsiConsole.MarkupLine("  [bold]--story[/] [italic]<dir>[/]       Directory containing .json scene files [dim][[required]][/]");
        AnsiConsole.MarkupLine("  [bold]--save[/]  [italic]<file>[/]      Save file path [dim][[default: save.json next to --story]][/]");
        AnsiConsole.MarkupLine("  [bold]--start[/] [italic]<scene-id>[/]  Starting scene when no save file exists");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold]COMMANDS[/]");
        AnsiConsole.MarkupLine("  [bold]edit[/]      Open the interactive story editor");
        AnsiConsole.MarkupLine("  [bold]reset[/]     Delete a save file");
        AnsiConsole.MarkupLine("  [bold]stats[/]     Print story graph statistics");
        AnsiConsole.MarkupLine("  [bold]inspect[/]   Print a debug snapshot of current game state");
        AnsiConsole.MarkupLine("  [bold]validate[/]  Validate story JSON and graph correctness");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold]PLAYER CONTROLS[/]");
        AnsiConsole.MarkupLine("  [bold]1[/]–[bold]9[/]   Select a choice");
        AnsiConsole.MarkupLine("  [bold]s[/]     Save progress");
        AnsiConsole.MarkupLine("  [bold]d[/]     Print debug inspect");
        AnsiConsole.MarkupLine("  [bold]q[/]     Save and quit");
    }

    internal static void PrintValidate()
    {
        AnsiConsole.MarkupLine("[bold yellow]dialogue-engine validate[/]");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold]USAGE[/]");
        AnsiConsole.WriteLine("  dialogue-engine validate --story <dir> [--start <scene-id>] [--format text|json] [--warnings-as-errors]");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold]OPTIONS[/]");
        AnsiConsole.MarkupLine("  [bold]--story[/] [italic]<dir>[/]           Story directory [dim][[required]][/]");
        AnsiConsole.MarkupLine("  [bold]--start[/] [italic]<scene-id>[/]      Override starting scene for graph validation");
        AnsiConsole.MarkupLine("  [bold]--format[/] [italic]text|json[/]      Output format [dim][[default: text]][/]");
        AnsiConsole.MarkupLine("  [bold]--warnings-as-errors[/]       Return non-zero when warnings are present");
        AnsiConsole.MarkupLine("  [bold]--help[/]                     Show validate help");
    }

    internal static void PrintUsage(string usage) =>
        AnsiConsole.MarkupLine($"[red]Usage:[/] {usage}");
}
