using DialogueGameEngine.Core.Tests.Helpers;

namespace DialogueGameEngine.Core.Tests;

public class EvaluationContextTests
{
    private static readonly AttributeAddress PlayerTrust =
        AttributeAddress.CharacterAttribute(TestFixture.Player, TestFixture.Trust);

    private static readonly FlagId MetMira = new("MetMira");

    [Fact]
    public void GetBaseValue_ReturnsRawStateValue()
    {
        var state = TestFixture.NewState();
        state.SetAttribute(PlayerTrust, 40);
        var modifier = new ModifierDefinition
        {
            Id = new ModifierId("boost"),
            Target = PlayerTrust,
            Delta = 20
        };
        var context = TestFixture.NewContext(state: state, modifiers: [modifier]);

        var result = context.GetBaseValue(PlayerTrust);

        ((int)result).Should().Be(40);
    }

    [Fact]
    public void GetEffectiveValue_AppliesModifierDelta()
    {
        var state = TestFixture.NewState();
        state.SetAttribute(PlayerTrust, 40);
        var modifier = new ModifierDefinition
        {
            Id = new ModifierId("boost"),
            Target = PlayerTrust,
            Delta = 20
        };
        var context = TestFixture.NewContext(state: state, modifiers: [modifier]);

        var result = context.GetEffectiveValue(PlayerTrust);

        ((int)result).Should().Be(60);
    }

    [Fact]
    public void GetEffectiveValue_MultipleModifiers_Stack()
    {
        var state = TestFixture.NewState();
        state.SetAttribute(PlayerTrust, 30);
        var modifiers = new[]
        {
            new ModifierDefinition { Id = new ModifierId("boost1"), Target = PlayerTrust, Delta = 10 },
            new ModifierDefinition { Id = new ModifierId("boost2"), Target = PlayerTrust, Delta = 15 }
        };
        var context = TestFixture.NewContext(state: state, modifiers: modifiers);

        var result = context.GetEffectiveValue(PlayerTrust);

        ((int)result).Should().Be(55);
    }

    [Fact]
    public void GetEffectiveValue_ConditionalModifier_AppliedWhenConditionMet()
    {
        var state = TestFixture.NewState();
        state.SetAttribute(PlayerTrust, 30);
        state.SetFlag(MetMira);
        var conditionalModifier = new ModifierDefinition
        {
            Id = new ModifierId("conditional_boost"),
            Target = PlayerTrust,
            Delta = 20,
            Condition = new FlagCondition { Flag = MetMira, Expected = true }
        };
        var context = TestFixture.NewContext(state: state, modifiers: [conditionalModifier]);

        var result = context.GetEffectiveValue(PlayerTrust);

        ((int)result).Should().Be(50);
    }

    [Fact]
    public void GetEffectiveValue_ConditionalModifier_NotAppliedWhenConditionNotMet()
    {
        var state = TestFixture.NewState();
        state.SetAttribute(PlayerTrust, 30);
        // MetMira flag NOT set
        var conditionalModifier = new ModifierDefinition
        {
            Id = new ModifierId("conditional_boost"),
            Target = PlayerTrust,
            Delta = 20,
            Condition = new FlagCondition { Flag = MetMira, Expected = true }
        };
        var context = TestFixture.NewContext(state: state, modifiers: [conditionalModifier]);

        var result = context.GetEffectiveValue(PlayerTrust);

        ((int)result).Should().Be(30);
    }

    [Fact]
    public void GetEffectiveValue_ModifierForDifferentAddress_NotApplied()
    {
        var state = TestFixture.NewState();
        state.SetAttribute(PlayerTrust, 40);
        var otherAddress = AttributeAddress.CharacterAttribute(TestFixture.Mira, TestFixture.Trust);
        var modifier = new ModifierDefinition
        {
            Id = new ModifierId("other_boost"),
            Target = otherAddress,
            Delta = 30
        };
        var context = TestFixture.NewContext(state: state, modifiers: [modifier]);

        var result = context.GetEffectiveValue(PlayerTrust);

        ((int)result).Should().Be(40);
    }

    [Fact]
    public void HasFlag_DelegatesToState()
    {
        var state = TestFixture.NewState();
        state.SetFlag(MetMira);
        var context = TestFixture.NewContext(state: state);

        context.HasFlag(MetMira).Should().BeTrue();
    }

    [Fact]
    public void HasFlag_FlagNotSet_ReturnsFalse()
    {
        var state = TestFixture.NewState();
        var context = TestFixture.NewContext(state: state);

        context.HasFlag(MetMira).Should().BeFalse();
    }
}
