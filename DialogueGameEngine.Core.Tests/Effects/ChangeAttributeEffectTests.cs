using DialogueGameEngine.Core.Tests.Helpers;

namespace DialogueGameEngine.Core.Tests;

public class ChangeAttributeEffectTests
{
    private static readonly AttributeAddress PlayerTrust =
        AttributeAddress.CharacterAttribute(TestFixture.Player, TestFixture.Trust);

    [Fact]
    public void Apply_PositiveDelta_AddsTrustValue()
    {
        var state = TestFixture.NewState();
        state.SetAttribute(PlayerTrust, 20);
        var context = TestFixture.NewContext(state: state);
        var effect = new ChangeAttributeEffect { Target = PlayerTrust, Delta = 15 };

        effect.Apply(state, context);

        ((int)state.GetAttribute(PlayerTrust)).Should().Be(35);
    }

    [Fact]
    public void Apply_NegativeDelta_SubtractsTrustValue()
    {
        var state = TestFixture.NewState();
        state.SetAttribute(PlayerTrust, 20);
        var context = TestFixture.NewContext(state: state);
        var effect = new ChangeAttributeEffect { Target = PlayerTrust, Delta = -10 };

        effect.Apply(state, context);

        ((int)state.GetAttribute(PlayerTrust)).Should().Be(10);
    }

    [Fact]
    public void Apply_DeltaThatExceedsMax_ClampsAt100()
    {
        var state = TestFixture.NewState();
        state.SetAttribute(PlayerTrust, 90);
        var context = TestFixture.NewContext(state: state);
        var effect = new ChangeAttributeEffect { Target = PlayerTrust, Delta = 50 };

        effect.Apply(state, context);

        ((int)state.GetAttribute(PlayerTrust)).Should().Be(AttributeValue.Max);
    }

    [Fact]
    public void Apply_DeltaThatGoesBelowMin_ClampsAtNeg100()
    {
        var state = TestFixture.NewState();
        state.SetAttribute(PlayerTrust, -90);
        var context = TestFixture.NewContext(state: state);
        var effect = new ChangeAttributeEffect { Target = PlayerTrust, Delta = -50 };

        effect.Apply(state, context);

        ((int)state.GetAttribute(PlayerTrust)).Should().Be(AttributeValue.Min);
    }

    [Fact]
    public void Apply_DeltaOnUnsetAttribute_TreatsBaseAsZero()
    {
        var state = TestFixture.NewState();
        var context = TestFixture.NewContext(state: state);
        var effect = new ChangeAttributeEffect { Target = PlayerTrust, Delta = 25 };

        effect.Apply(state, context);

        ((int)state.GetAttribute(PlayerTrust)).Should().Be(25);
    }
}
