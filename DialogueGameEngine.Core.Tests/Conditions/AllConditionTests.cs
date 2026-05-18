using DialogueGameEngine.Core.Tests.Helpers;

namespace DialogueGameEngine.Core.Tests;

public class AllConditionTests
{
    private static readonly EvaluationContext DefaultContext = TestFixture.NewContext();

    private static ICondition AlwaysTrue() => new FlagCondition { Flag = new FlagId("never_set"), Expected = false };
    private static ICondition AlwaysFalse() => new FlagCondition { Flag = new FlagId("never_set"), Expected = true };

    [Fact]
    public void EmptyList_ReturnsTrue()
    {
        var condition = new AllCondition { Conditions = [] };

        var result = condition.IsMet(DefaultContext);

        result.Should().BeTrue();
    }

    [Fact]
    public void AllTrue_ReturnsTrue()
    {
        var condition = new AllCondition
        {
            Conditions = [AlwaysTrue(), AlwaysTrue(), AlwaysTrue()]
        };

        var result = condition.IsMet(DefaultContext);

        result.Should().BeTrue();
    }

    [Fact]
    public void OneFalse_ReturnsFalse()
    {
        var condition = new AllCondition
        {
            Conditions = [AlwaysTrue(), AlwaysFalse(), AlwaysTrue()]
        };

        var result = condition.IsMet(DefaultContext);

        result.Should().BeFalse();
    }

    [Fact]
    public void ShortCircuits_WhenFirstConditionIsFalse()
    {
        var evaluated = false;
        var trackingCondition = new TrackingCondition(() =>
        {
            evaluated = true;
            return true;
        });

        var condition = new AllCondition
        {
            Conditions = [AlwaysFalse(), trackingCondition]
        };

        condition.IsMet(DefaultContext);

        evaluated.Should().BeFalse("AllCondition should short-circuit after the first false condition");
    }

    private sealed class TrackingCondition(Func<bool> evaluate) : ICondition
    {
        public bool IsMet(EvaluationContext context) => evaluate();
    }
}
