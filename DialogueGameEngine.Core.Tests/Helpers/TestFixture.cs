namespace DialogueGameEngine.Core.Tests.Helpers;

internal static class TestFixture
{
    internal static readonly CharacterId Player = new("Player");
    internal static readonly CharacterId Mira = new("Mira");
    internal static readonly AttributeId Trust = new("Trust");
    internal static readonly AttributeId Openness = new("Openness");
    internal static readonly SceneId SceneA = new("scene_a");
    internal static readonly SceneId SceneB = new("scene_b");

    internal static GameState NewState(SceneId? scene = null) =>
        new(scene ?? SceneA);

    internal static EvaluationContext NewContext(
        GameState? state = null,
        SceneDefinition? scene = null,
        IReadOnlyList<ModifierDefinition>? modifiers = null)
    {
        state ??= NewState();
        scene ??= NewScene();
        return new EvaluationContext
        {
            State = state,
            Scene = scene,
            ActiveModifiers = modifiers ?? []
        };
    }

    internal static SceneDefinition NewScene(
        SceneId? id = null,
        IReadOnlyList<ChoiceDefinition>? choices = null,
        IReadOnlyList<ModifierDefinition>? modifiers = null) =>
        new()
        {
            Id = id ?? SceneA,
            Title = "Test Scene",
            Choices = choices ?? [],
            Modifiers = modifiers ?? []
        };

    internal static ChoiceDefinition NewChoice(
        string id = "choice_a",
        ICondition? condition = null,
        IReadOnlyList<IEffect>? effects = null,
        SceneId? nextScene = null) =>
        new()
        {
            Id = new ChoiceId(id),
            Text = "A choice",
            Condition = condition,
            Effects = effects ?? [],
            NextScene = nextScene
        };
}
