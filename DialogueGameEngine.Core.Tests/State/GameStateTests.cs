using DialogueGameEngine.Core.Tests.Helpers;

namespace DialogueGameEngine.Core.Tests;

public class GameStateTests
{
    private static readonly AttributeAddress PlayerTrust =
        AttributeAddress.CharacterAttribute(TestFixture.Player, TestFixture.Trust);

    private static readonly FlagId MetMira = new("MetMira");

    [Fact]
    public void GetAttribute_UnknownAddress_ReturnsZero()
    {
        var state = TestFixture.NewState();

        var result = state.GetAttribute(PlayerTrust);

        ((int)result).Should().Be(0);
    }

    [Fact]
    public void SetAttribute_StoresAndRetrieves()
    {
        var state = TestFixture.NewState();

        state.SetAttribute(PlayerTrust, 25);

        ((int)state.GetAttribute(PlayerTrust)).Should().Be(25);
    }

    [Fact]
    public void SetAttribute_Overwrites_PreviousValue()
    {
        var state = TestFixture.NewState();
        state.SetAttribute(PlayerTrust, 25);

        state.SetAttribute(PlayerTrust, 50);

        ((int)state.GetAttribute(PlayerTrust)).Should().Be(50);
    }

    [Fact]
    public void AddToAttribute_Accumulates()
    {
        var state = TestFixture.NewState();
        state.SetAttribute(PlayerTrust, 10);

        state.AddToAttribute(PlayerTrust, 15);

        ((int)state.GetAttribute(PlayerTrust)).Should().Be(25);
    }

    [Fact]
    public void AddToAttribute_ClampsAtMax()
    {
        var state = TestFixture.NewState();
        state.SetAttribute(PlayerTrust, 95);

        state.AddToAttribute(PlayerTrust, 20);

        ((int)state.GetAttribute(PlayerTrust)).Should().Be(AttributeValue.Max);
    }

    [Fact]
    public void AddToAttribute_ClampsAtMin()
    {
        var state = TestFixture.NewState();
        state.SetAttribute(PlayerTrust, -95);

        state.AddToAttribute(PlayerTrust, -20);

        ((int)state.GetAttribute(PlayerTrust)).Should().Be(AttributeValue.Min);
    }

    [Fact]
    public void HasFlag_FalseByDefault()
    {
        var state = TestFixture.NewState();

        state.HasFlag(MetMira).Should().BeFalse();
    }

    [Fact]
    public void SetFlag_MakesHasFlagTrue()
    {
        var state = TestFixture.NewState();

        state.SetFlag(MetMira);

        state.HasFlag(MetMira).Should().BeTrue();
    }

    [Fact]
    public void ClearFlag_MakesHasFlagFalse()
    {
        var state = TestFixture.NewState();
        state.SetFlag(MetMira);

        state.ClearFlag(MetMira);

        state.HasFlag(MetMira).Should().BeFalse();
    }

    [Fact]
    public void MoveTo_ChangesCurrentScene()
    {
        var state = TestFixture.NewState(TestFixture.SceneA);

        state.MoveTo(TestFixture.SceneB);

        state.CurrentScene.Should().Be(TestFixture.SceneB);
    }

    [Fact]
    public void GetAllAttributes_ReflectsMutations()
    {
        var state = TestFixture.NewState();
        var addr2 = AttributeAddress.CharacterAttribute(TestFixture.Mira, TestFixture.Trust);
        state.SetAttribute(PlayerTrust, 10);
        state.SetAttribute(addr2, 20);

        var all = state.GetAllAttributes().ToDictionary(kvp => kvp.Key, kvp => (int)kvp.Value);

        all.Should().ContainKey(PlayerTrust).WhoseValue.Should().Be(10);
        all.Should().ContainKey(addr2).WhoseValue.Should().Be(20);
    }

    [Fact]
    public void GetAllFlags_ReflectsMutations()
    {
        var state = TestFixture.NewState();
        var flag2 = new FlagId("AnotherFlag");
        state.SetFlag(MetMira);
        state.SetFlag(flag2);

        var all = state.GetAllFlags().ToList();

        all.Should().Contain(MetMira);
        all.Should().Contain(flag2);
    }

    [Fact]
    public void GetAllFlags_DoesNotContainClearedFlag()
    {
        var state = TestFixture.NewState();
        state.SetFlag(MetMira);
        state.ClearFlag(MetMira);

        var all = state.GetAllFlags().ToList();

        all.Should().NotContain(MetMira);
    }
}
