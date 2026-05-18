namespace DialogueGameEngine.Cli.Editor.Prompts;

using DialogueGameEngine.Core;
using Spectre.Console;

internal static class AttributeAddressPrompt
{
    internal static AttributeAddress Ask(string title = "Target attribute:")
    {
        AnsiConsole.MarkupLine($"[bold]{Markup.Escape(title)}[/]");

        var scope = AnsiConsole.Prompt(
            new SelectionPrompt<AttributeScope>()
                .Title("Scope:")
                .AddChoices(Enum.GetValues<AttributeScope>())
                .UseConverter(s => s switch
                {
                    AttributeScope.Character => "Character  — belongs to one character",
                    AttributeScope.Relation  => "Relation   — directed between two characters",
                    AttributeScope.Scene     => "Scene      — anchored to a specific scene",
                    AttributeScope.World     => "World      — global, shared across all scenes",
                    _                        => s.ToString()
                }));

        switch (scope)
        {
            case AttributeScope.Character:
                var character = AnsiConsole.Prompt(new TextPrompt<string>("Character ID:"));
                var charAttr  = AnsiConsole.Prompt(new TextPrompt<string>("Attribute name (PascalCase):"));
                return AttributeAddress.CharacterAttribute(new CharacterId(character), new AttributeId(charAttr));

            case AttributeScope.Relation:
                var from    = AnsiConsole.Prompt(new TextPrompt<string>("From character:"));
                var to      = AnsiConsole.Prompt(new TextPrompt<string>("To character:"));
                var relAttr = AnsiConsole.Prompt(new TextPrompt<string>("Attribute name (PascalCase):"));
                return AttributeAddress.RelationAttribute(new CharacterId(from), new CharacterId(to), new AttributeId(relAttr));

            case AttributeScope.Scene:
                var sceneId   = AnsiConsole.Prompt(new TextPrompt<string>("Scene ID:"));
                var sceneAttr = AnsiConsole.Prompt(new TextPrompt<string>("Attribute name (PascalCase):"));
                return AttributeAddress.SceneAttribute(new SceneId(sceneId), new AttributeId(sceneAttr));

            case AttributeScope.World:
            default:
                var worldAttr = AnsiConsole.Prompt(new TextPrompt<string>("Attribute name (PascalCase):"));
                return AttributeAddress.WorldAttribute(new AttributeId(worldAttr));
        }
    }

    internal static string Format(AttributeAddress addr) => addr.Scope switch
    {
        AttributeScope.Character => $"{addr.Character!.Value}.{addr.Attribute.Value}",
        AttributeScope.Relation  => $"{addr.From!.Value}→{addr.To!.Value}.{addr.Attribute.Value}",
        AttributeScope.Scene     => $"[{addr.Scene!.Value}].{addr.Attribute.Value}",
        AttributeScope.World     => $"World.{addr.Attribute.Value}",
        _                        => addr.Attribute.Value
    };
}
