using DialogueGameEngine.Core.Tests.Helpers;

namespace DialogueGameEngine.Core.Tests;

public class FlagConditionTests
{
    private static readonly FlagId MetMira = new("MetMira");

    [Fact]
    public void FlagSet_ExpectedTrue_ReturnsTrue()
    {
        var state = TestFixture.NewState();
        state.SetFlag(MetMira);
        var context = TestFixture.NewContext(state: state);
        var condition = new FlagCondition { Flag = MetMira, Expected = true };

        var result = condition.IsMet(context);

        result.Should().BeTrue();
    }

    [Fact]
    public void FlagNotSet_ExpectedTrue_ReturnsFalse()
    {
        var state = TestFixture.NewState();
        var context = TestFixture.NewContext(state: state);
        var condition = new FlagCondition { Flag = MetMira, Expected = true };

        var result = condition.IsMet(context);

        result.Should().BeFalse();
    }

    [Fact]
    public void FlagSet_ExpectedFalse_ReturnsFalse()
    {
        var state = TestFixture.NewState();
        state.SetFlag(MetMira);
        var context = TestFixture.NewContext(state: state);
        var condition = new FlagCondition { Flag = MetMira, Expected = false };

        var result = condition.IsMet(context);

        result.Should().BeFalse();
    }

    [Fact]
    public void FlagNotSet_ExpectedFalse_ReturnsTrue()
    {
        var state = TestFixture.NewState();
        var context = TestFixture.NewContext(state: state);
        var condition = new FlagCondition { Flag = MetMira, Expected = false };

        var result = condition.IsMet(context);

        result.Should().BeTrue();
    }
}
