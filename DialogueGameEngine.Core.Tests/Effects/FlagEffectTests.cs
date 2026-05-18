using DialogueGameEngine.Core.Tests.Helpers;

namespace DialogueGameEngine.Core.Tests;

public class FlagEffectTests
{
    private static readonly FlagId MetMira = new("MetMira");

    [Fact]
    public void SetFlagEffect_SetsFlag()
    {
        var state = TestFixture.NewState();
        var context = TestFixture.NewContext(state: state);
        var effect = new SetFlagEffect { Flag = MetMira };

        effect.Apply(state, context);

        state.HasFlag(MetMira).Should().BeTrue();
    }

    [Fact]
    public void ClearFlagEffect_ClearsFlag()
    {
        var state = TestFixture.NewState();
        state.SetFlag(MetMira);
        var context = TestFixture.NewContext(state: state);
        var effect = new ClearFlagEffect { Flag = MetMira };

        effect.Apply(state, context);

        state.HasFlag(MetMira).Should().BeFalse();
    }

    [Fact]
    public void ClearFlagEffect_NonExistentFlag_IsNoOp()
    {
        var state = TestFixture.NewState();
        var context = TestFixture.NewContext(state: state);
        var effect = new ClearFlagEffect { Flag = MetMira };

        var act = () => effect.Apply(state, context);

        act.Should().NotThrow();
        state.HasFlag(MetMira).Should().BeFalse();
    }
}
