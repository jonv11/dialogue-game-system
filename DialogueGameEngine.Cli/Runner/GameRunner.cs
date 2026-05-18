namespace DialogueGameEngine.Cli.Runner;

using DialogueGameEngine.Cli.Display;
using DialogueGameEngine.Core;
using DialogueGameEngine.Persistence.Repositories;
using Spectre.Console;

/// <summary>
/// Drives the main play loop: render scene → show choices → read input → apply choice → repeat.
/// </summary>
internal sealed class GameRunner
{
    private readonly DialogueEngine _engine;
    private readonly IGameStateRepository _saveRepo;

    internal GameRunner(DialogueEngine engine, IGameStateRepository saveRepo)
    {
        _engine = engine;
        _saveRepo = saveRepo;
    }

    internal void Run(GameState state)
    {
        // Clear() and FigletText fail when stdout is not an interactive terminal (e.g. piped input).
        try { AnsiConsole.Clear(); } catch { /* not interactive — no-op */ }
        try { AnsiConsole.Write(new FigletText("Dialogue Engine").Color(Color.Yellow)); } catch { /* no-op */ }

        while (true)
        {
            var scene = _engine.GetCurrentScene(state);
            SceneRenderer.Render(scene, state);

            var choices = _engine.GetAvailableChoices(state);
            ChoiceRenderer.Render(choices);

            if (choices.Count == 0)
            {
                AnsiConsole.MarkupLine("[bold yellow]The story has ended here.[/]");
                _saveRepo.Save(state);
                break;
            }

            var input = Console.ReadLine()?.Trim().ToLowerInvariant() ?? string.Empty;

            if (input == "q")
            {
                _saveRepo.Save(state);
                AnsiConsole.MarkupLine("[dim]Progress saved. Goodbye.[/]");
                break;
            }

            if (input == "s")
            {
                _saveRepo.Save(state);
                AnsiConsole.MarkupLine("[dim]Saved.[/]");
                continue;
            }

            if (!int.TryParse(input, out var index) || index < 1 || index > choices.Count)
            {
                AnsiConsole.MarkupLine("[red]Invalid input. Enter a number from the list.[/]");
                continue;
            }

            var chosen = choices[index - 1];
            _engine.SelectChoice(state, chosen.Id);

            // Auto-save after every choice so progress is never lost
            _saveRepo.Save(state);
        }
    }
}
