namespace DialogueGameEngine.Cli.Display;

using DialogueGameEngine.Core;
using DialogueGameEngine.Core.Analysis;
using Spectre.Console;

internal static class StoryStatsRenderer
{
    internal static void Render(StoryGraphStats stats, SceneId startScene)
    {
        AnsiConsole.WriteLine();

        var table = new Table()
            .AddColumn("[bold]Metric[/]")
            .AddColumn(new TableColumn("[bold]Value[/]").RightAligned())
            .HideHeaders()
            .Border(TableBorder.None);

        table.AddRow("[dim]Starting scene[/]",      $"[yellow]{Markup.Escape(startScene.Value)}[/]");
        table.AddRow("[dim]Total scenes[/]",         stats.TotalScenes.ToString());
        table.AddRow("[dim]Total choices[/]",        stats.TotalChoices.ToString());
        table.AddRow("[dim]Ending scenes[/]",        stats.EndingScenes.ToString());
        table.AddRow("[dim]Reachable endings[/]",    stats.ReachableEndings.ToString());
        table.AddRow("[dim]Unreachable scenes[/]",   Highlight(stats.UnreachableScenes, warn: stats.UnreachableScenes > 0));
        table.AddRow("[dim]Orphan scenes[/]",        Highlight(stats.OrphanScenes, warn: stats.OrphanScenes > 0));

        var pathStr = stats.PathCountExceedsLimit
            ? $"[yellow]>{stats.UniquePathCount:N0}[/] [dim](cap reached)[/]"
            : stats.UniquePathCount.ToString("N0");
        table.AddRow("[dim]Unique paths[/]",         pathStr);

        table.AddRow("[dim]Avg choices / scene[/]",  $"{stats.AvgChoicesPerScene:F1}");
        table.AddRow("[dim]Max choices in scene[/]", stats.MaxChoicesInScene.ToString());
        table.AddRow("[dim]Min choices in scene[/]", stats.MinChoicesInScene.ToString());

        AnsiConsole.Write(new Panel(table)
        {
            Header = new PanelHeader("[bold yellow] Story Graph Statistics [/]"),
            Border = BoxBorder.Rounded,
            Padding = new Padding(1, 0)
        });

        if (stats.BrokenNextSceneRefs.Count > 0)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[red bold]Broken scene references:[/]");
            foreach (var broken in stats.BrokenNextSceneRefs)
                AnsiConsole.MarkupLine($"  [red]scene reference[/] [dim]→[/] [yellow]{Markup.Escape(broken)}[/] [dim](no scene with this id)[/]");
        }

        AnsiConsole.WriteLine();
    }

    private static string Highlight(int value, bool warn) =>
        warn ? $"[red]{value}[/]" : value.ToString();
}
