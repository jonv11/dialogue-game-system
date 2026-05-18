using DialogueGameEngine.Core.Tests.Helpers;

namespace DialogueGameEngine.Core.Tests;

public class MoveToSceneEffectTests
{
    [Fact]
    public void Apply_ChangesCurrentSceneToEffectScene()
    {
        var state = TestFixture.NewState(TestFixture.SceneA);
        var context = TestFixture.NewContext(state: state);
        var effect = new MoveToSceneEffect { Scene = TestFixture.SceneB };

        effect.Apply(state, context);

        state.CurrentScene.Should().Be(TestFixture.SceneB);
    }

    [Fact]
    public void Apply_SameScene_CurrentSceneUnchanged()
    {
        var state = TestFixture.NewState(TestFixture.SceneA);
        var context = TestFixture.NewContext(state: state);
        var effect = new MoveToSceneEffect { Scene = TestFixture.SceneA };

        effect.Apply(state, context);

        state.CurrentScene.Should().Be(TestFixture.SceneA);
    }
}
