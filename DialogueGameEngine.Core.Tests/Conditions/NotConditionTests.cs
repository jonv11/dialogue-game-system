using DialogueGameEngine.Core.Tests.Helpers;

namespace DialogueGameEngine.Core.Tests;

public class NotConditionTests
{
    private static readonly EvaluationContext DefaultContext = TestFixture.NewContext();

    private static ICondition AlwaysTrue() => new FlagCondition { Flag = new FlagId("never_set"), Expected = false };
    private static ICondition AlwaysFalse() => new FlagCondition { Flag = new FlagId("never_set"), Expected = true };

    [Fact]
    public void WrappingTrueCondition_ReturnsFalse()
    {
        var condition = new NotCondition { Condition = AlwaysTrue() };

        var result = condition.IsMet(DefaultContext);

        result.Should().BeFalse();
    }

    [Fact]
    public void WrappingFalseCondition_ReturnsTrue()
    {
        var condition = new NotCondition { Condition = AlwaysFalse() };

        var result = condition.IsMet(DefaultContext);

        result.Should().BeTrue();
    }
}
