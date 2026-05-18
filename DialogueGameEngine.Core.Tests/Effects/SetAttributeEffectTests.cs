using DialogueGameEngine.Core.Tests.Helpers;

namespace DialogueGameEngine.Core.Tests;

public class SetAttributeEffectTests
{
    private static readonly AttributeAddress PlayerTrust =
        AttributeAddress.CharacterAttribute(TestFixture.Player, TestFixture.Trust);

    [Fact]
    public void Apply_SetsExactValue()
    {
        var state = TestFixture.NewState();
        state.SetAttribute(PlayerTrust, 10);
        var context = TestFixture.NewContext(state: state);
        var effect = new SetAttributeEffect { Target = PlayerTrust, Value = 55 };

        effect.Apply(state, context);

        ((int)state.GetAttribute(PlayerTrust)).Should().Be(55);
    }

    [Fact]
    public void Apply_ValueAboveMax_ClampsAt100()
    {
        var state = TestFixture.NewState();
        var context = TestFixture.NewContext(state: state);
        var effect = new SetAttributeEffect { Target = PlayerTrust, Value = 150 };

        effect.Apply(state, context);

        ((int)state.GetAttribute(PlayerTrust)).Should().Be(AttributeValue.Max);
    }

    [Fact]
    public void Apply_ValueBelowMin_ClampsAtNeg100()
    {
        var state = TestFixture.NewState();
        var context = TestFixture.NewContext(state: state);
        var effect = new SetAttributeEffect { Target = PlayerTrust, Value = -150 };

        effect.Apply(state, context);

        ((int)state.GetAttribute(PlayerTrust)).Should().Be(AttributeValue.Min);
    }

    [Fact]
    public void Apply_OverwritesPreviousValue()
    {
        var state = TestFixture.NewState();
        state.SetAttribute(PlayerTrust, 80);
        var context = TestFixture.NewContext(state: state);
        var effect = new SetAttributeEffect { Target = PlayerTrust, Value = 20 };

        effect.Apply(state, context);

        ((int)state.GetAttribute(PlayerTrust)).Should().Be(20);
    }
}
