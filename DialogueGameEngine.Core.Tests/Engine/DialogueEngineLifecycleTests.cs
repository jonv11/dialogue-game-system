using DialogueGameEngine.Core.Tests.Helpers;

namespace DialogueGameEngine.Core.Tests;

public class DialogueEngineLifecycleTests
{
    private static readonly AttributeAddress CharacterTrust =
        AttributeAddress.CharacterAttribute(TestFixture.Player, TestFixture.Trust);
    private static readonly AttributeAddress RelationTrust =
        AttributeAddress.RelationAttribute(TestFixture.Player, TestFixture.Mira, TestFixture.Trust);
    private static readonly AttributeAddress SceneTension =
        AttributeAddress.SceneAttribute(TestFixture.SceneA, new AttributeId("Tension"));
    private static readonly AttributeAddress WorldPressure =
        AttributeAddress.WorldAttribute(new AttributeId("Pressure"));

    [Fact]
    public void Start_RunsInitialSceneOnEnterOnce()
    {
        var scene = TestFixture.NewScene(
            TestFixture.SceneA,
            onEnterEffects: [new ChangeAttributeEffect { Target = CharacterTrust, Delta = 5 }]);
        var engine = new DialogueEngine([scene]);
        var state = TestFixture.NewState(TestFixture.SceneA);

        engine.Start(state);
        engine.Start(state);
        _ = engine.GetCurrentScene(state);
        _ = engine.GetAvailableChoices(state);

        state.HasStarted.Should().BeTrue();
        ((int)state.GetAttribute(CharacterTrust)).Should().Be(5);
    }

    [Fact]
    public void SelectChoice_AppliesExitChoiceAndTargetEnterInOrder()
    {
        var order = new List<string>();
        var choice = TestFixture.NewChoice(
            "go",
            effects: [new RecordingEffect("choice", order)],
            nextScene: TestFixture.SceneB);
        var sceneA = TestFixture.NewScene(
            TestFixture.SceneA,
            choices: [choice],
            onExitEffects: [new RecordingEffect("exit", order)]);
        var sceneB = TestFixture.NewScene(
            TestFixture.SceneB,
            onEnterEffects: [new RecordingEffect("enter", order)]);
        var engine = new DialogueEngine([sceneA, sceneB]);
        var state = TestFixture.NewState(TestFixture.SceneA);

        engine.SelectChoice(state, new ChoiceId("go"));

        order.Should().Equal("exit", "choice", "enter");
        state.CurrentScene.Should().Be(TestFixture.SceneB);
    }

    [Fact]
    public void TargetOnEnter_RunsAfterStateMovesToTargetScene()
    {
        var choice = TestFixture.NewChoice("go", nextScene: TestFixture.SceneB);
        var sceneA = TestFixture.NewScene(TestFixture.SceneA, choices: [choice]);
        var sceneB = TestFixture.NewScene(
            TestFixture.SceneB,
            onEnterEffects: [new AssertCurrentSceneEffect(TestFixture.SceneB)]);
        var engine = new DialogueEngine([sceneA, sceneB]);
        var state = TestFixture.NewState(TestFixture.SceneA);

        var act = () => engine.SelectChoice(state, new ChoiceId("go"));

        act.Should().NotThrow();
    }

    [Fact]
    public void ReturningToScene_RerunsOnEnter()
    {
        var toB = TestFixture.NewChoice("to_b", nextScene: TestFixture.SceneB);
        var toA = TestFixture.NewChoice("to_a", nextScene: TestFixture.SceneA);
        var sceneA = TestFixture.NewScene(
            TestFixture.SceneA,
            choices: [toB],
            onEnterEffects: [new ChangeAttributeEffect { Target = CharacterTrust, Delta = 1 }]);
        var sceneB = TestFixture.NewScene(TestFixture.SceneB, choices: [toA]);
        var engine = new DialogueEngine([sceneA, sceneB]);
        var state = TestFixture.NewState(TestFixture.SceneA);

        engine.Start(state);
        engine.SelectChoice(state, new ChoiceId("to_b"));
        engine.SelectChoice(state, new ChoiceId("to_a"));

        ((int)state.GetAttribute(CharacterTrust)).Should().Be(2);
        state.CurrentScene.Should().Be(TestFixture.SceneA);
    }

    [Fact]
    public void LifecycleEffects_ModifyAllAttributeScopesAndFlags()
    {
        var entered = new FlagId("Entered");
        var scene = TestFixture.NewScene(
            TestFixture.SceneA,
            onEnterEffects:
            [
                new ChangeAttributeEffect { Target = CharacterTrust, Delta = 1 },
                new ChangeAttributeEffect { Target = RelationTrust, Delta = 2 },
                new ChangeAttributeEffect { Target = SceneTension, Delta = 3 },
                new ChangeAttributeEffect { Target = WorldPressure, Delta = 4 },
                new SetFlagEffect { Flag = entered }
            ]);
        var engine = new DialogueEngine([scene]);
        var state = TestFixture.NewState(TestFixture.SceneA);

        engine.Start(state);

        ((int)state.GetAttribute(CharacterTrust)).Should().Be(1);
        ((int)state.GetAttribute(RelationTrust)).Should().Be(2);
        ((int)state.GetAttribute(SceneTension)).Should().Be(3);
        ((int)state.GetAttribute(WorldPressure)).Should().Be(4);
        state.HasFlag(entered).Should().BeTrue();
    }

    [Fact]
    public void ConditionsAfterLifecycleEffects_ObserveUpdatedState()
    {
        var exited = new FlagId("Exited");
        var choiceSawExit = new FlagId("ChoiceSawExit");
        var enterSawChoice = new FlagId("EnterSawChoice");
        var choice = TestFixture.NewChoice(
            "go",
            effects:
            [
                new ConditionalEffect
                {
                    Condition = new FlagCondition { Flag = exited, Expected = true },
                    Then = [new SetFlagEffect { Flag = choiceSawExit }],
                    Else = []
                }
            ],
            nextScene: TestFixture.SceneB);
        var sceneA = TestFixture.NewScene(
            TestFixture.SceneA,
            choices: [choice],
            onExitEffects: [new SetFlagEffect { Flag = exited }]);
        var sceneB = TestFixture.NewScene(
            TestFixture.SceneB,
            onEnterEffects:
            [
                new ConditionalEffect
                {
                    Condition = new FlagCondition { Flag = choiceSawExit, Expected = true },
                    Then = [new SetFlagEffect { Flag = enterSawChoice }],
                    Else = []
                }
            ]);
        var engine = new DialogueEngine([sceneA, sceneB]);
        var state = TestFixture.NewState(TestFixture.SceneA);

        engine.SelectChoice(state, new ChoiceId("go"));

        state.HasFlag(exited).Should().BeTrue();
        state.HasFlag(choiceSawExit).Should().BeTrue();
        state.HasFlag(enterSawChoice).Should().BeTrue();
    }

    private sealed class RecordingEffect(string label, List<string> order) : IEffect
    {
        public void Apply(GameState state, EvaluationContext context) => order.Add(label);
    }

    private sealed class AssertCurrentSceneEffect(SceneId expectedScene) : IEffect
    {
        public void Apply(GameState state, EvaluationContext context)
        {
            state.CurrentScene.Should().Be(expectedScene);
            context.Scene.Id.Should().Be(expectedScene);
        }
    }
}

