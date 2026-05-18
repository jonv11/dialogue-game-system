namespace DialogueGameEngine.Cli.Display;

using DialogueGameEngine.Core;
using Spectre.Console;

/// <summary>Renders a scene definition to the terminal using Spectre.Console panels.</summary>
internal static class SceneRenderer
{
    internal static void Render(SceneDefinition scene, GameState state)
    {
        AnsiConsole.WriteLine();

        // Build the body text shown inside the panel
        var body = new Markup(scene.Description is not null
            ? $"[grey]{Markup.Escape(scene.Description)}[/]"
            : "[grey italic](No description)[/]");

        var panel = new Panel(body)
        {
            Header = new PanelHeader($"[bold yellow]{Markup.Escape(scene.Title)}[/]"),
            Border = BoxBorder.Rounded,
            Padding = new Padding(1, 0)
        };

        AnsiConsole.Write(panel);

        // Show participants if any are declared
        if (scene.Participants.Count > 0)
        {
            var participants = string.Join(", ", scene.Participants.Select(p => p.Value));
            AnsiConsole.MarkupLine($"  [dim]Present:[/] [cyan]{Markup.Escape(participants)}[/]");
        }

        AnsiConsole.WriteLine();
    }
}
