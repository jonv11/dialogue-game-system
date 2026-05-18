using DialogueGameEngine.Core.Tests.Helpers;

namespace DialogueGameEngine.Core.Tests;

public class AttributeAddressTests
{
    [Fact]
    public void CharacterAttribute_SetsCorrectProperties()
    {
        var address = AttributeAddress.CharacterAttribute(TestFixture.Player, TestFixture.Trust);

        address.Scope.Should().Be(AttributeScope.Character);
        address.Attribute.Should().Be(TestFixture.Trust);
        address.Character.Should().Be(TestFixture.Player);
        address.From.Should().BeNull();
        address.To.Should().BeNull();
        address.Scene.Should().BeNull();
    }

    [Fact]
    public void RelationAttribute_SetsFromAndTo()
    {
        var address = AttributeAddress.RelationAttribute(TestFixture.Player, TestFixture.Mira, TestFixture.Trust);

        address.Scope.Should().Be(AttributeScope.Relation);
        address.Attribute.Should().Be(TestFixture.Trust);
        address.From.Should().Be(TestFixture.Player);
        address.To.Should().Be(TestFixture.Mira);
        address.Character.Should().BeNull();
        address.Scene.Should().BeNull();
    }

    [Fact]
    public void SceneAttribute_SetsScene()
    {
        var address = AttributeAddress.SceneAttribute(TestFixture.SceneA, TestFixture.Openness);

        address.Scope.Should().Be(AttributeScope.Scene);
        address.Attribute.Should().Be(TestFixture.Openness);
        address.Scene.Should().Be(TestFixture.SceneA);
        address.Character.Should().BeNull();
        address.From.Should().BeNull();
        address.To.Should().BeNull();
    }

    [Fact]
    public void WorldAttribute_LeavesCharacterSceneFromToNull()
    {
        var address = AttributeAddress.WorldAttribute(TestFixture.Trust);

        address.Scope.Should().Be(AttributeScope.World);
        address.Attribute.Should().Be(TestFixture.Trust);
        address.Character.Should().BeNull();
        address.From.Should().BeNull();
        address.To.Should().BeNull();
        address.Scene.Should().BeNull();
    }

    [Fact]
    public void TwoIdenticalCharacterAttributeCalls_AreEqual()
    {
        var a = AttributeAddress.CharacterAttribute(TestFixture.Player, TestFixture.Trust);
        var b = AttributeAddress.CharacterAttribute(TestFixture.Player, TestFixture.Trust);

        a.Should().Be(b);
    }

    [Fact]
    public void TwoIdenticalRelationAttributeCalls_AreEqual()
    {
        var a = AttributeAddress.RelationAttribute(TestFixture.Player, TestFixture.Mira, TestFixture.Trust);
        var b = AttributeAddress.RelationAttribute(TestFixture.Player, TestFixture.Mira, TestFixture.Trust);

        a.Should().Be(b);
    }

    [Fact]
    public void TwoIdenticalWorldAttributeCalls_AreEqual()
    {
        var a = AttributeAddress.WorldAttribute(TestFixture.Trust);
        var b = AttributeAddress.WorldAttribute(TestFixture.Trust);

        a.Should().Be(b);
    }

    [Fact]
    public void UsedAsDictionaryKey_WorksCorrectly()
    {
        var address = AttributeAddress.CharacterAttribute(TestFixture.Player, TestFixture.Trust);
        var dict = new Dictionary<AttributeAddress, int> { [address] = 42 };

        var lookup = AttributeAddress.CharacterAttribute(TestFixture.Player, TestFixture.Trust);

        dict[lookup].Should().Be(42);
    }

    [Fact]
    public void DifferentFactories_AreNotEqual()
    {
        var charAttr = AttributeAddress.CharacterAttribute(TestFixture.Player, TestFixture.Trust);
        var worldAttr = AttributeAddress.WorldAttribute(TestFixture.Trust);

        charAttr.Should().NotBe(worldAttr);
    }
}
