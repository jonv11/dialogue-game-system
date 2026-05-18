namespace DialogueGameEngine.Cli.Editor;

using DialogueGameEngine.Cli.Editor.Prompts;
using DialogueGameEngine.Core;
using Spectre.Console;

internal sealed class ChoiceEditorMenu
{
    private ChoiceDefinition _choice;
    private readonly IReadOnlyList<string> _knownSceneIds;

    internal ChoiceEditorMenu(ChoiceDefinition choice, IReadOnlyList<string> knownSceneIds)
    {
        _choice = choice;
        _knownSceneIds = knownSceneIds;
    }

    // Returns null if the choice was deleted.
    internal ChoiceDefinition? Run()
    {
        while (true)
        {
            RenderHeader();

            var action = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .AddChoices(BuildMenu()));

            if (action.StartsWith("Edit text"))
                EditText();
            else if (action.StartsWith("Set condition"))
                SetCondition();
            else if (action.StartsWith("Manage effects"))
                ManageEffects();
            else if (action.StartsWith("Set next scene"))
                SetNextScene();
            else if (action == "Delete choice")
            {
                if (AnsiConsole.Confirm($"Delete choice '{_choice.Id.Value}'?", defaultValue: false))
                    return null;
            }
            else if (action == "Back")
                return _choice;
        }
    }

    private void RenderHeader()
    {
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine($"[bold]Choice:[/] {Markup.Escape(_choice.Id.Value)}");
        AnsiConsole.MarkupLine($"  Text:       {Markup.Escape(_choice.Text)}");
        AnsiConsole.MarkupLine($"  Condition:  {Markup.Escape(ConditionPrompt.Describe(_choice.Condition))}");
        AnsiConsole.MarkupLine($"  Effects:    {_choice.Effects.Count}");
        AnsiConsole.MarkupLine($"  Next scene: {_choice.NextScene?.Value ?? "(stay in scene)"}");
        AnsiConsole.WriteLine();
    }

    private string[] BuildMenu() =>
    [
        $"Edit text",
        $"Set condition   [{Markup.Escape(ConditionPrompt.Describe(_choice.Condition))}]",
        $"Manage effects  ({_choice.Effects.Count})",
        $"Set next scene  [{_choice.NextScene?.Value ?? "stay in scene"}]",
        "Delete choice",
        "Back",
    ];

    private void EditText()
    {
        var text = AnsiConsole.Prompt(
            new TextPrompt<string>("Choice text:")
                .DefaultValue(_choice.Text));
        _choice = _choice with { Text = text };
    }

    private void SetCondition()
    {
        var condition = ConditionPrompt.Ask(ConditionPrompt.Describe(_choice.Condition));
        _choice = _choice with { Condition = condition };
    }

    private void ManageEffects()
    {
        var effects = EffectPrompt.ManageList(_choice.Effects, $"Effects for choice: {_choice.Id.Value}");
        _choice = _choice with { Effects = effects };
    }

    private void SetNextScene()
    {
        var options = new List<string> { "(stay in current scene)" };
        options.AddRange(_knownSceneIds);
        options.Add("(type custom ID)");

        var selected = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Next scene after this choice:")
                .PageSize(20)
                .AddChoices(options));

        if (selected == "(stay in current scene)")
            _choice = _choice with { NextScene = null };
        else if (selected == "(type custom ID)")
        {
            var customId = AnsiConsole.Prompt(new TextPrompt<string>("Scene ID:"));
            _choice = _choice with { NextScene = new SceneId(customId) };
        }
        else
            _choice = _choice with { NextScene = new SceneId(selected) };
    }
}
