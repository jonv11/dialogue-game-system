using DialogueGameEngine.Core.Tests.Helpers;

namespace DialogueGameEngine.Core.Tests;

public class DialogueEngineTests
{
    private static readonly AttributeAddress PlayerTrust =
        AttributeAddress.CharacterAttribute(TestFixture.Player, TestFixture.Trust);

    private static readonly FlagId MetMira = new("MetMira");

    private static ICondition AlwaysTrue() => new FlagCondition { Flag = new FlagId("never_set"), Expected = false };
    private static ICondition AlwaysFalse() => new FlagCondition { Flag = new FlagId("never_set"), Expected = true };

    [Fact]
    public void GetCurrentScene_UnknownScene_ThrowsInvalidOperationException()
    {
        var engine = new DialogueEngine([TestFixture.NewScene(TestFixture.SceneA)]);
        var state = TestFixture.NewState(TestFixture.SceneB); // SceneB not registered

        var act = () => engine.GetCurrentScene(state);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void GetAvailableChoices_NoConditions_ReturnsAllChoices()
    {
        var choices = new[]
        {
            TestFixture.NewChoice("choice_1"),
            TestFixture.NewChoice("choice_2"),
            TestFixture.NewChoice("choice_3")
        };
        var scene = TestFixture.NewScene(TestFixture.SceneA, choices: choices);
        var engine = new DialogueEngine([scene]);
        var state = TestFixture.NewState(TestFixture.SceneA);

        var available = engine.GetAvailableChoices(state);

        available.Should().HaveCount(3);
    }

    [Fact]
    public void GetAvailableChoices_FiltersChoicesWhoseConditionIsNotMet()
    {
        var choices = new[]
        {
            TestFixture.NewChoice("visible", condition: AlwaysTrue()),
            TestFixture.NewChoice("hidden", condition: AlwaysFalse())
        };
        var scene = TestFixture.NewScene(TestFixture.SceneA, choices: choices);
        var engine = new DialogueEngine([scene]);
        var state = TestFixture.NewState(TestFixture.SceneA);

        var available = engine.GetAvailableChoices(state);

        available.Should().HaveCount(1);
        available[0].Id.Should().Be(new ChoiceId("visible"));
    }

    [Fact]
    public void SelectChoice_AppliesEffectsToState()
    {
        var choiceId = new ChoiceId("choice_a");
        var effects = new IEffect[] { new ChangeAttributeEffect { Target = PlayerTrust, Delta = 15 } };
        var choice = TestFixture.NewChoice("choice_a", effects: effects);
        var scene = TestFixture.NewScene(TestFixture.SceneA, choices: [choice]);
        var engine = new DialogueEngine([scene]);
        var state = TestFixture.NewState(TestFixture.SceneA);

        engine.SelectChoice(state, choiceId);

        ((int)state.GetAttribute(PlayerTrust)).Should().Be(15);
    }

    [Fact]
    public void SelectChoice_TransitionsToNextScene()
    {
        var choice = TestFixture.NewChoice("choice_a", nextScene: TestFixture.SceneB);
        var sceneA = TestFixture.NewScene(TestFixture.SceneA, choices: [choice]);
        var sceneB = TestFixture.NewScene(TestFixture.SceneB);
        var engine = new DialogueEngine([sceneA, sceneB]);
        var state = TestFixture.NewState(TestFixture.SceneA);

        engine.SelectChoice(state, new ChoiceId("choice_a"));

        state.CurrentScene.Should().Be(TestFixture.SceneB);
    }

    [Fact]
    public void SelectChoice_NoNextScene_CurrentSceneUnchanged()
    {
        var choice = TestFixture.NewChoice("choice_a", nextScene: null);
        var scene = TestFixture.NewScene(TestFixture.SceneA, choices: [choice]);
        var engine = new DialogueEngine([scene]);
        var state = TestFixture.NewState(TestFixture.SceneA);

        engine.SelectChoice(state, new ChoiceId("choice_a"));

        state.CurrentScene.Should().Be(TestFixture.SceneA);
    }

    [Fact]
    public void SelectChoice_UnknownChoiceId_Throws()
    {
        var scene = TestFixture.NewScene(TestFixture.SceneA, choices: []);
        var engine = new DialogueEngine([scene]);
        var state = TestFixture.NewState(TestFixture.SceneA);

        var act = () => engine.SelectChoice(state, new ChoiceId("nonexistent"));

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void SelectChoice_ChoiceConditionNotMet_Throws()
    {
        var choice = TestFixture.NewChoice("hidden_choice", condition: AlwaysFalse());
        var scene = TestFixture.NewScene(TestFixture.SceneA, choices: [choice]);
        var engine = new DialogueEngine([scene]);
        var state = TestFixture.NewState(TestFixture.SceneA);

        var act = () => engine.SelectChoice(state, new ChoiceId("hidden_choice"));

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void FullFlow_TwoScenes_ChoiceUpdatesStateAndTransitions()
    {
        // Arrange: scene A has a choice that raises trust and moves to scene B
        var trustEffect = new ChangeAttributeEffect { Target = PlayerTrust, Delta = 20 };
        var meetMiraEffect = new SetFlagEffect { Flag = MetMira };
        var choice = TestFixture.NewChoice(
            "greet_mira",
            effects: [trustEffect, meetMiraEffect],
            nextScene: TestFixture.SceneB);
        var sceneA = TestFixture.NewScene(TestFixture.SceneA, choices: [choice]);
        var sceneB = TestFixture.NewScene(TestFixture.SceneB);
        var engine = new DialogueEngine([sceneA, sceneB]);
        var state = TestFixture.NewState(TestFixture.SceneA);

        // Act
        engine.SelectChoice(state, new ChoiceId("greet_mira"));

        // Assert: effects applied and scene transitioned
        ((int)state.GetAttribute(PlayerTrust)).Should().Be(20);
        state.HasFlag(MetMira).Should().BeTrue();
        state.CurrentScene.Should().Be(TestFixture.SceneB);

        // Assert: engine correctly resolves the new scene
        var currentScene = engine.GetCurrentScene(state);
        currentScene.Id.Should().Be(TestFixture.SceneB);
    }

    [Fact]
    public void GetCurrentScene_KnownScene_ReturnsCorrectScene()
    {
        var scene = TestFixture.NewScene(TestFixture.SceneA);
        var engine = new DialogueEngine([scene]);
        var state = TestFixture.NewState(TestFixture.SceneA);

        var result = engine.GetCurrentScene(state);

        result.Id.Should().Be(TestFixture.SceneA);
    }

    [Fact]
    public void CreateContext_IncludesOnlyCurrentSceneModifiers()
    {
        var currentSceneModifier = new ModifierDefinition
        {
            Id = new ModifierId("active_boost"),
            Target = PlayerTrust,
            Delta = 10,
            Duration = ModifierDuration.CurrentScene
        };
        var instantModifier = new ModifierDefinition
        {
            Id = new ModifierId("instant_boost"),
            Target = PlayerTrust,
            Delta = 20,
            Duration = ModifierDuration.Instant
        };
        var scene = TestFixture.NewScene(
            TestFixture.SceneA,
            modifiers: [currentSceneModifier, instantModifier]);
        var engine = new DialogueEngine([scene]);
        var state = TestFixture.NewState(TestFixture.SceneA);

        var context = engine.CreateContext(state);

        context.Scene.Should().Be(scene);
        context.ActiveModifiers.Should().ContainSingle()
            .Which.Id.Should().Be(new ModifierId("active_boost"));
        ((int)context.GetEffectiveValue(PlayerTrust)).Should().Be(10);
    }

    [Fact]
    public void GetAvailableChoices_NullCondition_ChoiceIsIncluded()
    {
        var choice = TestFixture.NewChoice("unconditional", condition: null);
        var scene = TestFixture.NewScene(TestFixture.SceneA, choices: [choice]);
        var engine = new DialogueEngine([scene]);
        var state = TestFixture.NewState(TestFixture.SceneA);

        var available = engine.GetAvailableChoices(state);

        available.Should().HaveCount(1);
    }

    [Fact]
    public void Constructor_DuplicateSceneIds_ThrowsStructuredException()
    {
        var first = TestFixture.NewScene(TestFixture.SceneA);
        var second = TestFixture.NewScene(TestFixture.SceneA);

        var act = () => new DialogueEngine(
        [
            new SceneDocument { Scene = first, FilePath = "first.json" },
            new SceneDocument { Scene = second, FilePath = "second.json" }
        ]);

        act.Should().Throw<StoryValidationException>()
            .Which.Issues.Should().Contain(i =>
                i.Code == StoryValidationCodes.DuplicateSceneId &&
                i.Message.Contains("scene_a") &&
                i.Message.Contains("first.json") &&
                i.Message.Contains("second.json"));
    }
}
