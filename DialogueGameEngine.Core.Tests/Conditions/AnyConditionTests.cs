using DialogueGameEngine.Core.Tests.Helpers;

namespace DialogueGameEngine.Core.Tests;

public class AnyConditionTests
{
    private static readonly EvaluationContext DefaultContext = TestFixture.NewContext();

    private static ICondition AlwaysTrue() => new FlagCondition { Flag = new FlagId("never_set"), Expected = false };
    private static ICondition AlwaysFalse() => new FlagCondition { Flag = new FlagId("never_set"), Expected = true };

    [Fact]
    public void EmptyList_ReturnsFalse()
    {
        var condition = new AnyCondition { Conditions = [] };

        var result = condition.IsMet(DefaultContext);

        result.Should().BeFalse();
    }

    [Fact]
    public void AllFalse_ReturnsFalse()
    {
        var condition = new AnyCondition
        {
            Conditions = [AlwaysFalse(), AlwaysFalse(), AlwaysFalse()]
        };

        var result = condition.IsMet(DefaultContext);

        result.Should().BeFalse();
    }

    [Fact]
    public void OneTrue_ReturnsTrue()
    {
        var condition = new AnyCondition
        {
            Conditions = [AlwaysFalse(), AlwaysTrue(), AlwaysFalse()]
        };

        var result = condition.IsMet(DefaultContext);

        result.Should().BeTrue();
    }

    [Fact]
    public void AllTrue_ReturnsTrue()
    {
        var condition = new AnyCondition
        {
            Conditions = [AlwaysTrue(), AlwaysTrue()]
        };

        var result = condition.IsMet(DefaultContext);

        result.Should().BeTrue();
    }
}
