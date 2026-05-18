namespace DialogueGameEngine.Core.Tests.Validation;

public class StoryValidatorTests
{
    [Fact]
    public void ValidMinimalStory_Passes()
    {
        var result = Validate(Doc(EndScene("start")));

        result.HasErrors.Should().BeFalse();
        result.Issues.Should().BeEmpty();
    }

    [Fact]
    public void DuplicateSceneIds_AreErrorsWithPaths()
    {
        var result = Validate(
            Doc(EndScene("start"), "a.json"),
            Doc(EndScene("start"), "b.json"));

        result.Issues.Should().Contain(i =>
            i.Code == StoryValidationCodes.DuplicateSceneId &&
            i.Message.Contains("start") &&
            i.Message.Contains("a.json") &&
            i.Message.Contains("b.json"));
    }

    [Fact]
    public void DuplicateChoiceIds_AreErrors()
    {
        var scene = Scene("start",
            TestFixture.NewChoice("same"),
            TestFixture.NewChoice("same"));

        var result = Validate(Doc(scene));

        result.Issues.Should().Contain(i =>
            i.Code == StoryValidationCodes.DuplicateChoiceId &&
            i.SceneId == "start" &&
            i.ChoiceId == "same");
    }

    [Fact]
    public void UnknownNextScene_IsError()
    {
        var result = Validate(Doc(Scene("start", TestFixture.NewChoice("go", nextScene: new SceneId("missing")))));

        result.Issues.Should().Contain(i =>
            i.Code == StoryValidationCodes.UnknownSceneReference &&
            i.Message.Contains("missing"));
    }

    [Fact]
    public void UnknownMoveToSceneEffectTarget_IsError()
    {
        var choice = TestFixture.NewChoice(
            "go",
            effects: [new MoveToSceneEffect { Scene = new SceneId("missing") }]);
        var result = Validate(Doc(Scene("start", choice)));

        result.Issues.Should().Contain(i =>
            i.Code == StoryValidationCodes.UnknownSceneReference &&
            i.Field == "effects");
    }

    [Fact]
    public void NestedConditionalMoveToSceneEffectTarget_IsError()
    {
        var choice = TestFixture.NewChoice(
            "go",
            effects:
            [
                new ConditionalEffect
                {
                    Condition = new FlagCondition { Flag = new FlagId("Flag") },
                    Then = [new MoveToSceneEffect { Scene = new SceneId("missing") }],
                    Else = []
                }
            ]);
        var result = Validate(Doc(Scene("start", choice)));

        result.Issues.Should().Contain(i =>
            i.Code == StoryValidationCodes.UnknownSceneReference &&
            i.Message.Contains("missing"));
    }

    [Fact]
    public void InvalidAttributeAddress_IsError()
    {
        var invalid = new AttributeAddress
        {
            Scope = AttributeScope.Relation,
            From = new CharacterId("A"),
            Attribute = new AttributeId("Trust")
        };
        var choice = TestFixture.NewChoice(
            "go",
            effects: [new ChangeAttributeEffect { Target = invalid, Delta = 1 }]);

        var result = Validate(Doc(Scene("start", choice)));

        result.Issues.Should().Contain(i =>
            i.Code == StoryValidationCodes.InvalidAttributeAddress &&
            i.Field != null &&
            i.Field.Contains(".to"));
    }

    [Fact]
    public void UnknownConditionType_IsError()
    {
        var choice = TestFixture.NewChoice("go", condition: new UnknownCondition());

        var result = Validate(Doc(Scene("start", choice)));

        result.Issues.Should().Contain(i => i.Code == StoryValidationCodes.UnknownConditionType);
    }

    [Fact]
    public void UnknownEffectType_IsError()
    {
        var choice = TestFixture.NewChoice("go", effects: [new UnknownEffect()]);

        var result = Validate(Doc(Scene("start", choice)));

        result.Issues.Should().Contain(i => i.Code == StoryValidationCodes.UnknownEffectType);
    }

    [Fact]
    public void InvalidInitialState_IsError()
    {
        var initial = new InitialStateDefinition
        {
            CurrentScene = "",
            Attributes = [new InitialAttributeDefinition { Address = null, Value = null }],
            Flags = [""]
        };

        var result = Validate([Doc(EndScene("start"))], initial);

        result.Issues.Should().Contain(i => i.Code == StoryValidationCodes.InvalidInitialState);
    }

    [Fact]
    public void UnreachableScene_IsWarning()
    {
        var result = Validate(Doc(EndScene("start")), Doc(EndScene("unused")));

        result.Issues.Should().Contain(i =>
            i.Severity == StoryValidationSeverity.Warning &&
            i.Code == StoryValidationCodes.UnreachableScene &&
            i.SceneId == "unused");
    }

    [Fact]
    public void NoPlayablePath_IsErrorWhenNoEndingReachable()
    {
        var result = Validate(
            Doc(Scene("a", TestFixture.NewChoice("to_b", nextScene: new SceneId("b")))),
            Doc(Scene("b", TestFixture.NewChoice("to_a", nextScene: new SceneId("a")))));

        result.Issues.Should().Contain(i =>
            i.Severity == StoryValidationSeverity.Error &&
            i.Code == StoryValidationCodes.NoPlayablePath);
    }

    [Fact]
    public void OutOfRangeAttributeValues_AreWarnings()
    {
        var initial = new InitialStateDefinition
        {
            CurrentScene = "start",
            Attributes =
            [
                new InitialAttributeDefinition
                {
                    Address = AttributeAddress.WorldAttribute(new AttributeId("Pressure")),
                    Value = 150
                }
            ]
        };

        var result = Validate([Doc(EndScene("start"))], initial);

        result.Issues.Should().Contain(i =>
            i.Severity == StoryValidationSeverity.Warning &&
            i.Code == StoryValidationCodes.AttributeValueOutOfRange);
        result.HasErrors.Should().BeFalse();
    }

    private static StoryValidationResult Validate(params SceneDocument[] documents) =>
        Validate(documents, initialState: null);

    private static StoryValidationResult Validate(
        IEnumerable<SceneDocument> documents,
        InitialStateDefinition? initialState) =>
        new StoryValidator().Validate(documents, initialState);

    private static SceneDocument Doc(SceneDefinition scene, string? path = null) =>
        new() { Scene = scene, FilePath = path };

    private static SceneDefinition EndScene(string id) =>
        TestFixture.NewScene(new SceneId(id), choices: []);

    private static SceneDefinition Scene(string id, params ChoiceDefinition[] choices) =>
        TestFixture.NewScene(new SceneId(id), choices: choices);

    private sealed class UnknownCondition : ICondition
    {
        public bool IsMet(EvaluationContext context) => true;
    }

    private sealed class UnknownEffect : IEffect
    {
        public void Apply(GameState state, EvaluationContext context)
        {
        }
    }
}
