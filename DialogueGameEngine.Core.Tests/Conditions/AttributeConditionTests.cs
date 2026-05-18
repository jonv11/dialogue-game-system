using DialogueGameEngine.Core.Tests.Helpers;

namespace DialogueGameEngine.Core.Tests;

public class AttributeConditionTests
{
    private static readonly AttributeAddress PlayerTrust =
        AttributeAddress.CharacterAttribute(TestFixture.Player, TestFixture.Trust);

    private static EvaluationContext ContextWithTrust(int value, IReadOnlyList<ModifierDefinition>? modifiers = null)
    {
        var state = TestFixture.NewState();
        state.SetAttribute(PlayerTrust, value);
        return TestFixture.NewContext(state: state, modifiers: modifiers);
    }

    [Theory]
    [InlineData(30, 50, true)]   // 30 < 50
    [InlineData(50, 50, false)]  // 50 < 50 is false
    [InlineData(70, 50, false)]  // 70 < 50 is false
    public void LessThan_ReturnsCorrectResult(int stateValue, int threshold, bool expected)
    {
        var condition = new AttributeCondition
        {
            Target = PlayerTrust,
            Operator = ComparisonOperator.LessThan,
            Value = threshold
        };
        var context = ContextWithTrust(stateValue);

        var result = condition.IsMet(context);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(30, 50, true)]   // 30 <= 50
    [InlineData(50, 50, true)]   // 50 <= 50
    [InlineData(70, 50, false)]  // 70 <= 50 is false
    public void LessThanOrEqual_ReturnsCorrectResult(int stateValue, int threshold, bool expected)
    {
        var condition = new AttributeCondition
        {
            Target = PlayerTrust,
            Operator = ComparisonOperator.LessThanOrEqual,
            Value = threshold
        };
        var context = ContextWithTrust(stateValue);

        var result = condition.IsMet(context);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(50, 50, true)]   // 50 == 50
    [InlineData(30, 50, false)]  // 30 == 50 is false
    [InlineData(70, 50, false)]  // 70 == 50 is false
    public void Equal_ReturnsCorrectResult(int stateValue, int threshold, bool expected)
    {
        var condition = new AttributeCondition
        {
            Target = PlayerTrust,
            Operator = ComparisonOperator.Equal,
            Value = threshold
        };
        var context = ContextWithTrust(stateValue);

        var result = condition.IsMet(context);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(70, 50, true)]   // 70 >= 50
    [InlineData(50, 50, true)]   // 50 >= 50
    [InlineData(30, 50, false)]  // 30 >= 50 is false
    public void GreaterThanOrEqual_ReturnsCorrectResult(int stateValue, int threshold, bool expected)
    {
        var condition = new AttributeCondition
        {
            Target = PlayerTrust,
            Operator = ComparisonOperator.GreaterThanOrEqual,
            Value = threshold
        };
        var context = ContextWithTrust(stateValue);

        var result = condition.IsMet(context);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(70, 50, true)]   // 70 > 50
    [InlineData(50, 50, false)]  // 50 > 50 is false
    [InlineData(30, 50, false)]  // 30 > 50 is false
    public void GreaterThan_ReturnsCorrectResult(int stateValue, int threshold, bool expected)
    {
        var condition = new AttributeCondition
        {
            Target = PlayerTrust,
            Operator = ComparisonOperator.GreaterThan,
            Value = threshold
        };
        var context = ContextWithTrust(stateValue);

        var result = condition.IsMet(context);

        result.Should().Be(expected);
    }

    [Fact]
    public void UseEffectiveValue_True_UsesModifierAdjustedValue()
    {
        // Base trust = 30, modifier adds 30 → effective = 60, threshold = 50
        var modifier = new ModifierDefinition
        {
            Id = new ModifierId("boost"),
            Target = PlayerTrust,
            Delta = 30
        };
        var context = ContextWithTrust(30, modifiers: [modifier]);
        var condition = new AttributeCondition
        {
            Target = PlayerTrust,
            Operator = ComparisonOperator.GreaterThan,
            Value = 50,
            UseEffectiveValue = true
        };

        var result = condition.IsMet(context);

        result.Should().BeTrue();
    }

    [Fact]
    public void UseEffectiveValue_False_IgnoresModifiers()
    {
        // Base trust = 30, modifier adds 30 → effective = 60, but using base value
        var modifier = new ModifierDefinition
        {
            Id = new ModifierId("boost"),
            Target = PlayerTrust,
            Delta = 30
        };
        var context = ContextWithTrust(30, modifiers: [modifier]);
        var condition = new AttributeCondition
        {
            Target = PlayerTrust,
            Operator = ComparisonOperator.GreaterThan,
            Value = 50,
            UseEffectiveValue = false
        };

        var result = condition.IsMet(context);

        result.Should().BeFalse();
    }
}
