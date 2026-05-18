namespace DialogueGameEngine.Cli.Editor.Prompts;

using DialogueGameEngine.Core;
using Spectre.Console;

internal static class EffectPrompt
{
    private enum EffectType
    {
        ChangeAttribute,
        SetAttribute,
        SetFlag,
        ClearFlag,
        MoveToScene,
        Conditional,
    }

    internal static IEffect? Ask()
    {
        var type = AnsiConsole.Prompt(
            new SelectionPrompt<EffectType>()
                .Title("Effect type:")
                .AddChoices(Enum.GetValues<EffectType>())
                .UseConverter(t => t switch
                {
                    EffectType.ChangeAttribute => "ChangeAttributeEffect — add delta to an attribute",
                    EffectType.SetAttribute    => "SetAttributeEffect    — set attribute to exact value",
                    EffectType.SetFlag         => "SetFlagEffect         — mark a flag as happened",
                    EffectType.ClearFlag       => "ClearFlagEffect       — remove a flag",
                    EffectType.MoveToScene     => "MoveToSceneEffect     — jump to a scene",
                    EffectType.Conditional     => "ConditionalEffect     — branch on a condition",
                    _                          => t.ToString()
                }));

        return type switch
        {
            EffectType.ChangeAttribute => AskChangeAttribute(),
            EffectType.SetAttribute    => AskSetAttribute(),
            EffectType.SetFlag         => AskSetFlag(),
            EffectType.ClearFlag       => AskClearFlag(),
            EffectType.MoveToScene     => AskMoveToScene(),
            EffectType.Conditional     => AskConditional(),
            _                          => null
        };
    }

    internal static string Describe(IEffect effect) => effect switch
    {
        ChangeAttributeEffect e => $"ChangeAttr: {AttributeAddressPrompt.Format(e.Target)} {(e.Delta >= 0 ? "+" : "")}{e.Delta}",
        SetAttributeEffect e    => $"SetAttr: {AttributeAddressPrompt.Format(e.Target)} = {e.Value}",
        SetFlagEffect e         => $"SetFlag: {e.Flag.Value}",
        ClearFlagEffect e       => $"ClearFlag: {e.Flag.Value}",
        MoveToSceneEffect e     => $"MoveToScene: {e.Scene.Value}",
        ConditionalEffect e     => $"Conditional: if ({ConditionPrompt.Describe(e.Condition)}) → {e.Then.Count} then, {e.Else.Count} else",
        _                       => effect.GetType().Name
    };

    internal static IReadOnlyList<IEffect> ManageList(IReadOnlyList<IEffect> existing, string title)
    {
        var list = existing.ToList();

        while (true)
        {
            AnsiConsole.Clear();
            AnsiConsole.MarkupLine($"[bold]{Markup.Escape(title)}[/]");

            for (int i = 0; i < list.Count; i++)
                AnsiConsole.MarkupLine($"  {i + 1}. {Markup.Escape(Describe(list[i]))}");

            AnsiConsole.WriteLine();

            var options = new List<string> { "[+] Add effect" };
            for (int i = 0; i < list.Count; i++)
                options.Add($"Remove [{i + 1}] {Describe(list[i])}");
            options.Add("Done");

            var action = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Action:")
                    .PageSize(15)
                    .AddChoices(options));

            if (action == "[+] Add effect")
            {
                var effect = Ask();
                if (effect is not null)
                    list.Add(effect);
            }
            else if (action.StartsWith("Remove ["))
            {
                var closeBracket = action.IndexOf(']');
                if (closeBracket > 8 && int.TryParse(action[8..closeBracket], out var num))
                    list.RemoveAt(num - 1);
            }
            else
            {
                break;
            }
        }

        return list;
    }

    private static ChangeAttributeEffect AskChangeAttribute()
    {
        var target = AttributeAddressPrompt.Ask("Attribute to change:");
        var delta  = AnsiConsole.Prompt(new TextPrompt<int>("Delta (positive = increase, negative = decrease):"));
        return new ChangeAttributeEffect { Target = target, Delta = delta };
    }

    private static SetAttributeEffect AskSetAttribute()
    {
        var target = AttributeAddressPrompt.Ask("Attribute to set:");
        var value  = AnsiConsole.Prompt(new TextPrompt<int>("Value (−100 to 100):"));
        return new SetAttributeEffect { Target = target, Value = value };
    }

    private static SetFlagEffect AskSetFlag()
    {
        var flag = AnsiConsole.Prompt(new TextPrompt<string>("Flag name (PastTense convention):"));
        return new SetFlagEffect { Flag = new FlagId(flag) };
    }

    private static ClearFlagEffect AskClearFlag()
    {
        var flag = AnsiConsole.Prompt(new TextPrompt<string>("Flag name to clear:"));
        return new ClearFlagEffect { Flag = new FlagId(flag) };
    }

    private static MoveToSceneEffect AskMoveToScene()
    {
        var scene = AnsiConsole.Prompt(new TextPrompt<string>("Target scene ID:"));
        return new MoveToSceneEffect { Scene = new SceneId(scene) };
    }

    private static ConditionalEffect AskConditional()
    {
        AnsiConsole.MarkupLine("[bold]ConditionalEffect — define condition:[/]");
        var condition = ConditionPrompt.Ask() ?? new FlagCondition { Flag = new FlagId("Placeholder"), Expected = true };

        AnsiConsole.MarkupLine("[bold]Then effects (when condition is met):[/]");
        var then = ManageList([], "Then effects");

        IReadOnlyList<IEffect> elseEffects = [];
        if (AnsiConsole.Confirm("Add else effects (when condition is NOT met)?", defaultValue: false))
            elseEffects = ManageList([], "Else effects");

        return new ConditionalEffect { Condition = condition, Then = then, Else = elseEffects };
    }
}
