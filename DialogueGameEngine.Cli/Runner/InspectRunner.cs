namespace DialogueGameEngine.Cli.Runner;

using DialogueGameEngine.Cli.Editor.Prompts;
using DialogueGameEngine.Core;

internal static class InspectRunner
{
    private const string Divider = "================================================================================";

    internal static void Run(DialogueEngine engine, GameState state, IEnumerable<SceneDefinition> allScenes)
    {
        var scenes = allScenes.OrderBy(s => s.Id.Value).ToList();
        var currentScene = scenes.FirstOrDefault(s => s.Id == state.CurrentScene);
        var context = currentScene is not null ? engine.CreateContext(state) : null;

        Console.WriteLine(Divider);
        Console.WriteLine("  DEBUG INSPECT");
        Console.WriteLine(Divider);
        Console.WriteLine();

        WriteCurrentState(state, currentScene);
        Console.WriteLine();

        if (currentScene is not null && context is not null)
            WriteCurrentSceneDetails(currentScene, context);
        else
        {
            Console.WriteLine("--- [2/3] CURRENT SCENE ---");
            Console.WriteLine();
            Console.WriteLine($"  (Scene '{state.CurrentScene.Value}' not found in loaded definitions)");
            Console.WriteLine();
        }

        WriteSceneList(scenes);
        Console.WriteLine(Divider);
    }

    private static void WriteCurrentState(GameState state, SceneDefinition? currentScene)
    {
        Console.WriteLine("--- [1/3] CURRENT STATE ---");
        Console.WriteLine();

        var sceneTitle = currentScene is not null ? $"  \"{currentScene.Title}\"" : "";
        Console.WriteLine($"  Scene:  {state.CurrentScene.Value}{sceneTitle}");
        Console.WriteLine();

        var flags = state.GetAllFlags().OrderBy(f => f.Value).ToList();
        if (flags.Count == 0)
            Console.WriteLine("  Flags:  (none)");
        else
        {
            Console.WriteLine("  Flags:");
            foreach (var flag in flags)
                Console.WriteLine($"    {flag.Value}");
        }
        Console.WriteLine();

        var attrs = state.GetAllAttributes().ToList();
        Console.WriteLine("  Attributes:");
        WriteAttributeGroup("World",     attrs.Where(a => a.Key.Scope == AttributeScope.World));
        WriteAttributeGroup("Character", attrs.Where(a => a.Key.Scope == AttributeScope.Character));
        WriteAttributeGroup("Relation",  attrs.Where(a => a.Key.Scope == AttributeScope.Relation));
        WriteAttributeGroup("Scene",     attrs.Where(a => a.Key.Scope == AttributeScope.Scene));
    }

    private static void WriteAttributeGroup(
        string label,
        IEnumerable<KeyValuePair<AttributeAddress, AttributeValue>> items)
    {
        var list = items.OrderBy(a => AttributeAddressPrompt.Format(a.Key)).ToList();
        var prefix = $"    [{label}]";
        const int prefixWidth = 16;

        if (list.Count == 0)
        {
            Console.WriteLine($"{prefix.PadRight(prefixWidth)} (none)");
            return;
        }

        for (int i = 0; i < list.Count; i++)
        {
            var addrStr  = AttributeAddressPrompt.Format(list[i].Key);
            var val      = list[i].Value.Value;
            var lineLabel = i == 0 ? prefix.PadRight(prefixWidth) : new string(' ', prefixWidth);
            Console.WriteLine($"{lineLabel} {addrStr} = {val}");
        }
    }

    private static void WriteCurrentSceneDetails(SceneDefinition scene, EvaluationContext context)
    {
        Console.WriteLine("--- [2/3] CURRENT SCENE ---");
        Console.WriteLine();
        Console.WriteLine($"  Title:        {scene.Title}");
        if (!string.IsNullOrWhiteSpace(scene.Description))
            Console.WriteLine($"  Description:  {scene.Description}");
        if (scene.Participants.Count > 0)
            Console.WriteLine($"  Participants: {string.Join(", ", scene.Participants.Select(p => p.Value))}");
        Console.WriteLine();

        var activeMods = context.ActiveModifiers;
        if (activeMods.Count == 0)
        {
            Console.WriteLine("  Modifiers:    (none)");
        }
        else
        {
            Console.WriteLine($"  Modifiers ({activeMods.Count} active, CurrentScene duration):");
            foreach (var mod in activeMods)
            {
                Console.WriteLine($"    {mod.Id.Value}");
                var addrStr     = AttributeAddressPrompt.Format(mod.Target);
                var baseVal     = context.GetBaseValue(mod.Target).Value;
                var effectiveVal = context.GetEffectiveValue(mod.Target).Value;
                var deltaStr    = mod.Delta >= 0 ? $"+{mod.Delta}" : mod.Delta.ToString();
                Console.WriteLine($"      Target:  {addrStr}");
                Console.WriteLine($"      Delta:   {deltaStr}   (base {baseVal} → effective {effectiveVal})");
                if (mod.Condition is not null)
                    Console.WriteLine($"      When:    {ConditionPrompt.Describe(mod.Condition)}");
            }
        }
        Console.WriteLine();

        if (scene.Choices.Count == 0)
        {
            Console.WriteLine("  Choices:      (none — ending scene)");
        }
        else
        {
            Console.WriteLine($"  Choices ({scene.Choices.Count}):");
            for (int i = 0; i < scene.Choices.Count; i++)
            {
                var choice    = scene.Choices[i];
                var available = choice.IsAvailable(context);
                var status    = available ? "[AVAILABLE]" : "[LOCKED]";
                Console.WriteLine();
                Console.WriteLine($"    [{i + 1}] {choice.Id.Value} — {choice.Text}   {status}");
                Console.WriteLine($"        Condition: {ConditionPrompt.Describe(choice.Condition)}");
                if (choice.Effects.Count > 0)
                {
                    Console.WriteLine("        Effects:");
                    foreach (var effect in choice.Effects)
                        Console.WriteLine($"          {EffectPrompt.Describe(effect)}");
                }
                else
                {
                    Console.WriteLine("        Effects:   (none)");
                }
                var nextStr = choice.NextScene?.Value ?? "(stay in scene)";
                Console.WriteLine($"        Next:      {nextStr}");
            }
        }
        Console.WriteLine();
    }

    private static void WriteSceneList(List<SceneDefinition> scenes)
    {
        Console.WriteLine($"--- [3/3] SCENE LIST ({scenes.Count} scenes) ---");
        Console.WriteLine();

        var idWidth = scenes.Count > 0 ? Math.Max(scenes.Max(s => s.Id.Value.Length), 8) : 8;
        foreach (var scene in scenes)
        {
            var choiceLabel = scene.Choices.Count == 1 ? "1 choice" : $"{scene.Choices.Count} choices";
            Console.WriteLine($"  {scene.Id.Value.PadRight(idWidth + 2)}  \"{scene.Title}\"   ({choiceLabel})");
        }
        Console.WriteLine();
    }
}
