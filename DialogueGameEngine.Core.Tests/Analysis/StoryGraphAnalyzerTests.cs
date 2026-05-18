using DialogueGameEngine.Core.Analysis;
using DialogueGameEngine.Core.Tests.Helpers;

namespace DialogueGameEngine.Core.Tests.Analysis;

public class StoryGraphAnalyzerTests
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    private static SceneDefinition Scene(string id, params string[] nextSceneIds)
    {
        var choices = nextSceneIds
            .Select((next, i) => TestFixture.NewChoice($"c{i}", nextScene: new SceneId(next)))
            .ToArray();
        return TestFixture.NewScene(new SceneId(id), choices: choices);
    }

    private static SceneDefinition EndScene(string id) =>
        TestFixture.NewScene(new SceneId(id), choices: []);

    private static StoryGraphStats Analyze(SceneDefinition[] scenes, string startId) =>
        new StoryGraphAnalyzer(scenes).Analyze(new SceneId(startId));

    // ── Basic counts ──────────────────────────────────────────────────────────

    [Fact]
    public void SingleScene_NoChoices_CorrectStats()
    {
        var result = Analyze([EndScene("a")], "a");

        result.TotalScenes.Should().Be(1);
        result.TotalChoices.Should().Be(0);
        result.EndingScenes.Should().Be(1);
        result.ReachableEndings.Should().Be(1);
        result.UniquePathCount.Should().Be(1);
        result.PathCountExceedsLimit.Should().BeFalse();
    }

    [Fact]
    public void TwoScenes_OneChoice_CorrectStats()
    {
        var scenes = new[] { Scene("a", "b"), EndScene("b") };
        var result = Analyze(scenes, "a");

        result.TotalScenes.Should().Be(2);
        result.TotalChoices.Should().Be(1);
        result.EndingScenes.Should().Be(1);
        result.ReachableEndings.Should().Be(1);
        result.OrphanScenes.Should().Be(0);
    }

    [Fact]
    public void TotalChoices_SumsAcrossAllScenes()
    {
        var sceneA = TestFixture.NewScene(new SceneId("a"), choices:
        [
            TestFixture.NewChoice("c1", nextScene: new SceneId("b")),
            TestFixture.NewChoice("c2", nextScene: new SceneId("c"))
        ]);
        var sceneB = TestFixture.NewScene(new SceneId("b"), choices:
        [
            TestFixture.NewChoice("c3", nextScene: new SceneId("d"))
        ]);
        var scenes = new[] { sceneA, sceneB, EndScene("c"), EndScene("d") };

        var result = Analyze(scenes, "a");

        result.TotalChoices.Should().Be(3);
    }

    // ── Reachability & orphans ────────────────────────────────────────────────

    [Fact]
    public void UnreachableScene_CountedCorrectly()
    {
        // A → B; C is disconnected
        var scenes = new[] { Scene("a", "b"), EndScene("b"), EndScene("c") };
        var result = Analyze(scenes, "a");

        result.UnreachableScenes.Should().Be(1);
    }

    [Fact]
    public void OrphanScene_NotStartScene_Counted()
    {
        // A → B; C has no incoming edges and is not the start
        var scenes = new[] { Scene("a", "b"), EndScene("b"), EndScene("c") };
        var result = Analyze(scenes, "a");

        result.OrphanScenes.Should().Be(1);
    }

    [Fact]
    public void StartScene_WithNoIncomingEdges_NotCountedAsOrphan()
    {
        // A is start with no incoming — should NOT be an orphan
        var scenes = new[] { Scene("a", "b"), EndScene("b") };
        var result = Analyze(scenes, "a");

        result.OrphanScenes.Should().Be(0);
    }

    [Fact]
    public void MultipleEndings_AllCounted()
    {
        // A → B (ending) and A → C (ending)
        var sceneA = TestFixture.NewScene(new SceneId("a"), choices:
        [
            TestFixture.NewChoice("c1", nextScene: new SceneId("b")),
            TestFixture.NewChoice("c2", nextScene: new SceneId("c"))
        ]);
        var scenes = new[] { sceneA, EndScene("b"), EndScene("c") };
        var result = Analyze(scenes, "a");

        result.EndingScenes.Should().Be(2);
        result.ReachableEndings.Should().Be(2);
    }

    // ── Path counting ──────────────────────────────────────────────────────────

    [Fact]
    public void LinearChain_OneUniquePath()
    {
        var scenes = new[] { Scene("a", "b"), Scene("b", "c"), EndScene("c") };
        var result = Analyze(scenes, "a");

        result.UniquePathCount.Should().Be(1);
        result.PathCountExceedsLimit.Should().BeFalse();
    }

    [Fact]
    public void Diamond_TwoUniquePaths()
    {
        // A → B → D  and  A → C → D
        var sceneA = TestFixture.NewScene(new SceneId("a"), choices:
        [
            TestFixture.NewChoice("c1", nextScene: new SceneId("b")),
            TestFixture.NewChoice("c2", nextScene: new SceneId("c"))
        ]);
        var scenes = new[] { sceneA, Scene("b", "d"), Scene("c", "d"), EndScene("d") };
        var result = Analyze(scenes, "a");

        result.UniquePathCount.Should().Be(2);
    }

    [Fact]
    public void BranchingTree_CorrectPathCount()
    {
        // A → B (end), A → C (end), A → D (end)
        var sceneA = TestFixture.NewScene(new SceneId("a"), choices:
        [
            TestFixture.NewChoice("c1", nextScene: new SceneId("b")),
            TestFixture.NewChoice("c2", nextScene: new SceneId("c")),
            TestFixture.NewChoice("c3", nextScene: new SceneId("d"))
        ]);
        var scenes = new[] { sceneA, EndScene("b"), EndScene("c"), EndScene("d") };
        var result = Analyze(scenes, "a");

        result.UniquePathCount.Should().Be(3);
    }

    [Fact]
    public void CyclicGraph_DoesNotHang()
    {
        // A → B → A  (pure cycle, no ending reachable)
        var scenes = new[] { Scene("a", "b"), Scene("b", "a") };
        var result = Analyze(scenes, "a");

        result.UniquePathCount.Should().Be(0);
        result.PathCountExceedsLimit.Should().BeFalse();
    }

    [Fact]
    public void CyclicGraph_WithExitBranch_CountsAcyclicPaths()
    {
        // A → B → A (cycle) and A → C (ending)
        var sceneA = TestFixture.NewScene(new SceneId("a"), choices:
        [
            TestFixture.NewChoice("loop", nextScene: new SceneId("b")),
            TestFixture.NewChoice("exit", nextScene: new SceneId("c"))
        ]);
        var scenes = new[] { sceneA, Scene("b", "a"), EndScene("c") };
        var result = Analyze(scenes, "a");

        result.UniquePathCount.Should().Be(1);
        result.PathCountExceedsLimit.Should().BeFalse();
    }

    [Fact]
    public void PathCount_CappedAtLimit_WhenTreeIsLarge()
    {
        // Binary tree of depth 14 → 2^14 = 16,384 paths > 10,000 cap
        var scenes = MakeBinaryTree(14).ToArray();
        var result = Analyze(scenes, "n0");

        result.PathCountExceedsLimit.Should().BeTrue();
        result.UniquePathCount.Should().Be(10_000);
    }

    // ── Broken references ──────────────────────────────────────────────────────

    [Fact]
    public void NoBrokenRefs_EmptyList()
    {
        var scenes = new[] { Scene("a", "b"), EndScene("b") };
        var result = Analyze(scenes, "a");

        result.BrokenNextSceneRefs.Should().BeEmpty();
    }

    [Fact]
    public void BrokenRef_ReportedInList()
    {
        var sceneA = TestFixture.NewScene(new SceneId("a"), choices:
        [
            TestFixture.NewChoice("bad", nextScene: new SceneId("ghost_scene"))
        ]);
        var result = Analyze([sceneA], "a");

        result.BrokenNextSceneRefs.Should().ContainSingle().Which.Should().Be("ghost_scene");
    }

    // ── Choices-per-scene stats ────────────────────────────────────────────────

    [Fact]
    public void ChoicesPerScene_StatsComputedCorrectly()
    {
        // Scenes with 0, 2, 3 choices
        var sceneA = TestFixture.NewScene(new SceneId("a"), choices:
        [
            TestFixture.NewChoice("c1", nextScene: new SceneId("b")),
            TestFixture.NewChoice("c2", nextScene: new SceneId("c"))
        ]);
        var sceneB = TestFixture.NewScene(new SceneId("b"), choices:
        [
            TestFixture.NewChoice("c3", nextScene: new SceneId("d")),
            TestFixture.NewChoice("c4", nextScene: new SceneId("d")),
            TestFixture.NewChoice("c5", nextScene: new SceneId("d"))
        ]);
        var scenes = new[] { sceneA, sceneB, EndScene("c"), EndScene("d") };
        var result = Analyze(scenes, "a");

        result.MinChoicesInScene.Should().Be(0);
        result.MaxChoicesInScene.Should().Be(3);
        result.AvgChoicesPerScene.Should().Be(Math.Round((0.0 + 0.0 + 2.0 + 3.0) / 4, 1));
    }

    // ── Edge case: empty scenes throws ────────────────────────────────────────

    [Fact]
    public void EmptyScenes_ThrowsArgumentException()
    {
        var act = () => new StoryGraphAnalyzer([]);
        act.Should().Throw<ArgumentException>();
    }

    // ── Binary tree generator ──────────────────────────────────────────────────

    private static IEnumerable<SceneDefinition> MakeBinaryTree(int depth)
    {
        // Generates a complete binary tree: node "n{i}" has children "n{2i+1}" and "n{2i+2}"
        // Leaves are at level `depth` (0-indexed) — they have no children.
        int totalNodes = (1 << (depth + 1)) - 1;
        int firstLeaf = (1 << depth) - 1;

        for (int i = 0; i < totalNodes; i++)
        {
            int left = 2 * i + 1;
            int right = 2 * i + 2;
            bool isLeaf = i >= firstLeaf;

            if (isLeaf)
            {
                yield return EndScene($"n{i}");
            }
            else
            {
                var choices = new[]
                {
                    TestFixture.NewChoice($"l{i}", nextScene: new SceneId($"n{left}")),
                    TestFixture.NewChoice($"r{i}", nextScene: new SceneId($"n{right}"))
                };
                yield return TestFixture.NewScene(new SceneId($"n{i}"), choices: choices);
            }
        }
    }
}
