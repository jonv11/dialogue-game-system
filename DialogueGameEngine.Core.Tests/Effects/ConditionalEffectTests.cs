using DialogueGameEngine.Core.Tests.Helpers;

namespace DialogueGameEngine.Core.Tests;

public class ConditionalEffectTests
{
    private static readonly AttributeAddress PlayerTrust =
        AttributeAddress.CharacterAttribute(TestFixture.Player, TestFixture.Trust);

    private static readonly FlagId MetMira = new("MetMira");

    private static ICondition AlwaysTrue() => new FlagCondition { Flag = new FlagId("never_set"), Expected = false };
    private static ICondition AlwaysFalse() => new FlagCondition { Flag = new FlagId("never_set"), Expected = true };

    [Fact]
    public void ConditionTrue_AppliesThenEffects_NotElse()
    {
        var state = TestFixture.NewState();
        var context = TestFixture.NewContext(state: state);
        var thenFlag = new FlagId("ThenApplied");
        var elseFlag = new FlagId("ElseApplied");
        var effect = new ConditionalEffect
        {
            Condition = AlwaysTrue(),
            Then = [new SetFlagEffect { Flag = thenFlag }],
            Else = [new SetFlagEffect { Flag = elseFlag }]
        };

        effect.Apply(state, context);

        state.HasFlag(thenFlag).Should().BeTrue();
        state.HasFlag(elseFlag).Should().BeFalse();
    }

    [Fact]
    public void ConditionFalse_AppliesElseEffects_NotThen()
    {
        var state = TestFixture.NewState();
        var context = TestFixture.NewContext(state: state);
        var thenFlag = new FlagId("ThenApplied");
        var elseFlag = new FlagId("ElseApplied");
        var effect = new ConditionalEffect
        {
            Condition = AlwaysFalse(),
            Then = [new SetFlagEffect { Flag = thenFlag }],
            Else = [new SetFlagEffect { Flag = elseFlag }]
        };

        effect.Apply(state, context);

        state.HasFlag(thenFlag).Should().BeFalse();
        state.HasFlag(elseFlag).Should().BeTrue();
    }

    [Fact]
    public void ConditionFalse_EmptyElse_NothingApplied()
    {
        var state = TestFixture.NewState();
        var context = TestFixture.NewContext(state: state);
        var thenFlag = new FlagId("ThenApplied");
        var effect = new ConditionalEffect
        {
            Condition = AlwaysFalse(),
            Then = [new SetFlagEffect { Flag = thenFlag }],
            Else = []
        };

        var act = () => effect.Apply(state, context);

        act.Should().NotThrow();
        state.HasFlag(thenFlag).Should().BeFalse();
    }

    [Fact]
    public void ConditionTrue_MultipleThenEffects_AllApplied()
    {
        var state = TestFixture.NewState();
        var context = TestFixture.NewContext(state: state);
        var effect = new ConditionalEffect
        {
            Condition = AlwaysTrue(),
            Then =
            [
                new ChangeAttributeEffect { Target = PlayerTrust, Delta = 10 },
                new SetFlagEffect { Flag = MetMira }
            ],
            Else = []
        };

        effect.Apply(state, context);

        ((int)state.GetAttribute(PlayerTrust)).Should().Be(10);
        state.HasFlag(MetMira).Should().BeTrue();
    }
}
