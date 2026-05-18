namespace DialogueGameEngine.Cli.Display;

using System.Text.Json;
using DialogueGameEngine.Core;
using Spectre.Console;

internal static class ValidationResultRenderer
{
    internal static void RenderText(StoryValidationResult result)
    {
        if (result.Issues.Count == 0)
        {
            AnsiConsole.MarkupLine("[green]Story is valid.[/]");
            return;
        }

        foreach (var issue in result.Issues.OrderByDescending(i => i.Severity).ThenBy(i => i.Code))
        {
            var severity = issue.Severity.ToString().ToUpperInvariant();
            var color = issue.Severity switch
            {
                StoryValidationSeverity.Error => "red",
                StoryValidationSeverity.Warning => "yellow",
                _ => "dim"
            };

            AnsiConsole.MarkupLine($"[{color}]{severity}[/] [bold]{Markup.Escape(issue.Code)}[/] {Markup.Escape(issue.Message)}");
            if (!string.IsNullOrWhiteSpace(issue.FilePath))
                AnsiConsole.MarkupLine($"  [dim]file:[/] {Markup.Escape(issue.FilePath)}");
            if (!string.IsNullOrWhiteSpace(issue.SceneId))
                AnsiConsole.MarkupLine($"  [dim]scene:[/] {Markup.Escape(issue.SceneId)}");
            if (!string.IsNullOrWhiteSpace(issue.ChoiceId))
                AnsiConsole.MarkupLine($"  [dim]choice:[/] {Markup.Escape(issue.ChoiceId)}");
            if (!string.IsNullOrWhiteSpace(issue.Field))
                AnsiConsole.MarkupLine($"  [dim]field:[/] {Markup.Escape(issue.Field)}");
        }
    }

    internal static void RenderJson(StoryValidationResult result)
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            WriteIndented = true
        };
        Console.WriteLine(JsonSerializer.Serialize(result, options));
    }
}

