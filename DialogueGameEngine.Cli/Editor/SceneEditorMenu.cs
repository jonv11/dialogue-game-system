namespace DialogueGameEngine.Cli.Editor;

using DialogueGameEngine.Cli.Editor.Prompts;
using DialogueGameEngine.Core;
using Spectre.Console;
using System.Text.Json;

internal sealed class SceneEditorMenu
{
    private SceneDefinition _scene;
    private readonly string _storyDir;
    private readonly JsonSerializerOptions _options;
    private readonly IReadOnlyList<string> _knownSceneIds;
    private bool _dirty;

    internal SceneEditorMenu(
        string storyDir,
        JsonSerializerOptions options,
        SceneDefinition scene,
        IReadOnlyList<string> knownSceneIds,
        bool dirty = false)
    {
        _storyDir = storyDir;
        _options = options;
        _scene = scene;
        _knownSceneIds = knownSceneIds;
        _dirty = dirty;
    }

    // Returns null if the scene was deleted.
    internal SceneDefinition? Run()
    {
        while (true)
        {
            RenderHeader();

            var action = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .AddChoices(BuildMenu()));

            if (action.StartsWith("Edit metadata"))
                EditMetadata();
            else if (action.StartsWith("Manage choices"))
                ManageChoices();
            else if (action.StartsWith("Manage ambiance"))
                ManageAmbiance();
            else if (action.StartsWith("Manage modifiers"))
                ManageModifiers();
            else if (action.StartsWith("On-enter effects"))
                ManageOnEnterEffects();
            else if (action.StartsWith("On-exit effects"))
                ManageOnExitEffects();
            else if (action == "Save to file")
                SaveScene();
            else if (action == "Delete scene")
            {
                if (AnsiConsole.Confirm($"Delete scene '{_scene.Id.Value}'? This removes the JSON file.", defaultValue: false))
                {
                    DeleteScene();
                    return null;
                }
            }
            else if (action == "Back")
                return _scene;
        }
    }

    private void RenderHeader()
    {
        AnsiConsole.Clear();
        var dirtyMark = _dirty ? " [yellow]*[/]" : string.Empty;
        AnsiConsole.MarkupLine($"[bold]Scene:[/] {Markup.Escape(_scene.Id.Value)}{dirtyMark}");
        AnsiConsole.MarkupLine($"  Title:        {Markup.Escape(_scene.Title)}");

        var descPreview = _scene.Description is null
            ? "[dim](none)[/]"
            : Markup.Escape(_scene.Description.Length > 60 ? _scene.Description[..60] + "…" : _scene.Description);
        AnsiConsole.MarkupLine($"  Description:  {descPreview}");

        var participants = _scene.Participants.Count == 0
            ? "[dim](none)[/]"
            : string.Join(", ", _scene.Participants.Select(p => p.Value));
        AnsiConsole.MarkupLine($"  Participants: {participants}");
        AnsiConsole.WriteLine();
    }

    private string[] BuildMenu() =>
    [
        "Edit metadata (title, description, participants)",
        $"Manage choices    ({_scene.Choices.Count})",
        $"Manage ambiance   ({_scene.Ambiance.Count} attributes)",
        $"Manage modifiers  ({_scene.Modifiers.Count})",
        $"On-enter effects  ({_scene.OnEnterEffects.Count})",
        $"On-exit effects   ({_scene.OnExitEffects.Count})",
        "Save to file",
        "Delete scene",
        "Back",
    ];

    private void EditMetadata()
    {
        var title = AnsiConsole.Prompt(
            new TextPrompt<string>("Title:")
                .DefaultValue(_scene.Title));

        var desc = AnsiConsole.Prompt(
            new TextPrompt<string>("Description (empty = none):")
                .AllowEmpty()
                .DefaultValue(_scene.Description ?? string.Empty));

        var participantsStr = AnsiConsole.Prompt(
            new TextPrompt<string>("Participants (comma-separated, empty = none):")
                .AllowEmpty()
                .DefaultValue(string.Join(", ", _scene.Participants.Select(p => p.Value))));

        var participants = participantsStr
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(p => new CharacterId(p))
            .ToList();

        _scene = _scene with
        {
            Title        = title,
            Description  = string.IsNullOrWhiteSpace(desc) ? null : desc,
            Participants = participants,
        };
        _dirty = true;
    }

    private void ManageChoices()
    {
        var choices = _scene.Choices.ToList();

        while (true)
        {
            AnsiConsole.Clear();
            AnsiConsole.MarkupLine("[bold]Choices[/]");

            for (int i = 0; i < choices.Count; i++)
                AnsiConsole.MarkupLine($"  {i + 1}. [{Markup.Escape(choices[i].Id.Value)}] {Markup.Escape(choices[i].Text)}");

            AnsiConsole.WriteLine();

            var menuItems = new List<string> { "[+] Add choice" };
            for (int i = 0; i < choices.Count; i++)
                menuItems.Add($"Edit [{i + 1}] {choices[i].Id.Value}");
            menuItems.Add("Back");

            var action = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .PageSize(20)
                    .AddChoices(menuItems));

            if (action == "[+] Add choice")
            {
                var id   = AnsiConsole.Prompt(new TextPrompt<string>("Choice ID (e.g. 'confess', 'leave'):"));
                var text = AnsiConsole.Prompt(new TextPrompt<string>("Choice text:"));
                var newChoice = new ChoiceDefinition { Id = new ChoiceId(id), Text = text };
                var menu = new ChoiceEditorMenu(newChoice, _knownSceneIds);
                var result = menu.Run();
                if (result is not null)
                    choices.Add(result);
            }
            else if (action.StartsWith("Edit ["))
            {
                var closeBracket = action.IndexOf(']');
                if (closeBracket > 6 && int.TryParse(action[6..closeBracket], out var num))
                {
                    var idx  = num - 1;
                    var menu = new ChoiceEditorMenu(choices[idx], _knownSceneIds);
                    var result = menu.Run();
                    if (result is null)
                        choices.RemoveAt(idx);
                    else
                        choices[idx] = result;
                }
            }
            else
                break;
        }

        _scene = _scene with { Choices = choices };
        _dirty = true;
    }

    private void ManageAmbiance()
    {
        var ambiance = _scene.Ambiance.ToList();

        while (true)
        {
            AnsiConsole.Clear();
            AnsiConsole.MarkupLine("[bold]Ambiance Attributes[/]");

            for (int i = 0; i < ambiance.Count; i++)
                AnsiConsole.MarkupLine($"  {i + 1}. {Markup.Escape(AttributeAddressPrompt.Format(ambiance[i].Target))} = {ambiance[i].Value}");

            AnsiConsole.WriteLine();

            var options = new List<string> { "[+] Add attribute" };
            for (int i = 0; i < ambiance.Count; i++)
                options.Add($"Remove [{i + 1}] {AttributeAddressPrompt.Format(ambiance[i].Target)}");
            options.Add("Back");

            var action = AnsiConsole.Prompt(
                new SelectionPrompt<string>().AddChoices(options));

            if (action == "[+] Add attribute")
            {
                var target = AttributeAddressPrompt.Ask();
                var value  = AnsiConsole.Prompt(new TextPrompt<int>("Value (−100 to 100):"));
                ambiance.Add(new AttributeAssignment { Target = target, Value = value });
            }
            else if (action.StartsWith("Remove ["))
            {
                var closeBracket = action.IndexOf(']');
                if (closeBracket > 8 && int.TryParse(action[8..closeBracket], out var num))
                    ambiance.RemoveAt(num - 1);
            }
            else
                break;
        }

        _scene = _scene with { Ambiance = ambiance };
        _dirty = true;
    }

    private void ManageModifiers()
    {
        var modifiers = _scene.Modifiers.ToList();

        while (true)
        {
            AnsiConsole.Clear();
            AnsiConsole.MarkupLine("[bold]Modifiers[/]");

            for (int i = 0; i < modifiers.Count; i++)
            {
                var m     = modifiers[i];
                var delta = m.Delta >= 0 ? $"+{m.Delta}" : $"{m.Delta}";
                AnsiConsole.MarkupLine($"  {i + 1}. [{Markup.Escape(m.Id.Value)}] {Markup.Escape(AttributeAddressPrompt.Format(m.Target))} {delta} ({m.Duration})");
            }

            AnsiConsole.WriteLine();

            var options = new List<string> { "[+] Add modifier" };
            for (int i = 0; i < modifiers.Count; i++)
                options.Add($"Remove [{i + 1}] {modifiers[i].Id.Value}");
            options.Add("Back");

            var action = AnsiConsole.Prompt(
                new SelectionPrompt<string>().AddChoices(options));

            if (action == "[+] Add modifier")
            {
                var id     = AnsiConsole.Prompt(new TextPrompt<string>("Modifier ID (snake_case):"));
                var target = AttributeAddressPrompt.Ask();
                var delta  = AnsiConsole.Prompt(new TextPrompt<int>("Delta (positive = increase, negative = decrease):"));

                var duration = AnsiConsole.Prompt(
                    new SelectionPrompt<ModifierDuration>()
                        .Title("Duration:")
                        .AddChoices(ModifierDuration.CurrentScene, ModifierDuration.Instant)
                        .UseConverter(d => d switch
                        {
                            ModifierDuration.CurrentScene => "CurrentScene — active while in this scene",
                            ModifierDuration.Instant      => "Instant      — applied once at scene entry",
                            _                             => d.ToString()
                        }));

                ICondition? condition = null;
                if (AnsiConsole.Confirm("Add a condition to gate this modifier?", defaultValue: false))
                    condition = ConditionPrompt.Ask();

                modifiers.Add(new ModifierDefinition
                {
                    Id        = new ModifierId(id),
                    Target    = target,
                    Delta     = delta,
                    Duration  = duration,
                    Condition = condition,
                });
            }
            else if (action.StartsWith("Remove ["))
            {
                var closeBracket = action.IndexOf(']');
                if (closeBracket > 8 && int.TryParse(action[8..closeBracket], out var num))
                    modifiers.RemoveAt(num - 1);
            }
            else
                break;
        }

        _scene = _scene with { Modifiers = modifiers };
        _dirty = true;
    }

    private void ManageOnEnterEffects()
    {
        var effects = EffectPrompt.ManageList(_scene.OnEnterEffects, "On-Enter Effects");
        _scene = _scene with { OnEnterEffects = effects };
        _dirty = true;
    }

    private void ManageOnExitEffects()
    {
        var effects = EffectPrompt.ManageList(_scene.OnExitEffects, "On-Exit Effects");
        _scene = _scene with { OnExitEffects = effects };
        _dirty = true;
    }

    private void SaveScene()
    {
        Directory.CreateDirectory(_storyDir);
        var path = Path.Combine(_storyDir, $"{_scene.Id.Value}.json");
        var json = JsonSerializer.Serialize(_scene, _options);
        File.WriteAllText(path, json);
        _dirty = false;
        AnsiConsole.MarkupLine($"[green]Saved:[/] {Markup.Escape(path)}");
        Console.ReadLine();
    }

    private void DeleteScene()
    {
        var path = Path.Combine(_storyDir, $"{_scene.Id.Value}.json");
        if (File.Exists(path))
            File.Delete(path);
    }
}
