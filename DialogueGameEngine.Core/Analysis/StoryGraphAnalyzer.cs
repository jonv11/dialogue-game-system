namespace DialogueGameEngine.Core.Analysis;

/// <summary>
/// Analyzes the directed graph of scenes and choices in a story.
/// </summary>
/// <remarks>
/// Graph edges are derived only from <see cref="ChoiceDefinition.NextScene"/> values.
/// Dynamic edges via <c>MoveToSceneEffect</c> inside choice effects are not statically
/// analyzable and are excluded.
/// </remarks>
public sealed class StoryGraphAnalyzer
{
    private const int PathCountCap = 10_000;

    private readonly IReadOnlyDictionary<SceneId, SceneDefinition> _scenes;

    public StoryGraphAnalyzer(IEnumerable<SceneDefinition> scenes)
    {
        _scenes = scenes.ToDictionary(s => s.Id);
        if (_scenes.Count == 0)
            throw new ArgumentException("Cannot analyze an empty story — no scenes were provided.", nameof(scenes));
    }

    /// <summary>
    /// Analyzes the story graph starting from <paramref name="startScene"/>.
    /// </summary>
    public StoryGraphStats Analyze(SceneId startScene)
    {
        // Build outgoing edge lists and in-degree counts.
        var outEdges = new Dictionary<SceneId, List<SceneId>>();
        var inDegree = new Dictionary<SceneId, int>();
        var brokenRefs = new SortedSet<string>();

        foreach (var id in _scenes.Keys)
        {
            outEdges[id] = [];
            inDegree.TryAdd(id, 0);
        }

        int totalChoices = 0;
        foreach (var scene in _scenes.Values)
        {
            totalChoices += scene.Choices.Count;
            foreach (var choice in scene.Choices)
            {
                if (choice.NextScene is not { } nextScene) continue;
                if (!_scenes.ContainsKey(nextScene))
                {
                    brokenRefs.Add(nextScene.Value);
                    continue;
                }
                outEdges[scene.Id].Add(nextScene);
                inDegree[nextScene] = inDegree.GetValueOrDefault(nextScene) + 1;
            }
        }

        // BFS reachability from start scene.
        var reachable = new HashSet<SceneId>();
        var queue = new Queue<SceneId>();
        if (_scenes.ContainsKey(startScene))
        {
            queue.Enqueue(startScene);
            reachable.Add(startScene);
        }
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            foreach (var next in outEdges[current])
            {
                if (reachable.Add(next))
                    queue.Enqueue(next);
            }
        }

        int endingScenes = _scenes.Keys.Count(id => outEdges[id].Count == 0);
        int reachableEndings = reachable.Count(id => outEdges[id].Count == 0);
        int unreachable = _scenes.Count - reachable.Count;
        int orphans = _scenes.Keys.Count(id => id != startScene && inDegree[id] == 0);

        // DFS path counting with on-stack cycle detection.
        int pathCount = 0;
        bool exceeded = false;
        var onStack = new HashSet<SceneId>();

        void CountPaths(SceneId node)
        {
            if (exceeded) return;
            if (outEdges[node].Count == 0)
            {
                pathCount++;
                if (pathCount >= PathCountCap) exceeded = true;
                return;
            }
            if (!onStack.Add(node)) return; // back-edge: cycle on current path — stop this branch
            foreach (var next in outEdges[node])
            {
                if (exceeded) break;
                CountPaths(next);
            }
            onStack.Remove(node);
        }

        if (_scenes.ContainsKey(startScene))
            CountPaths(startScene);

        // Choices-per-scene stats across all scenes.
        var choiceCounts = _scenes.Values.Select(s => s.Choices.Count).ToList();
        double avg = choiceCounts.Count > 0 ? choiceCounts.Average() : 0;
        int max = choiceCounts.Count > 0 ? choiceCounts.Max() : 0;
        int min = choiceCounts.Count > 0 ? choiceCounts.Min() : 0;

        return new StoryGraphStats
        {
            TotalScenes           = _scenes.Count,
            TotalChoices          = totalChoices,
            EndingScenes          = endingScenes,
            OrphanScenes          = orphans,
            ReachableEndings      = reachableEndings,
            UnreachableScenes     = unreachable,
            UniquePathCount       = pathCount,
            PathCountExceedsLimit = exceeded,
            AvgChoicesPerScene    = Math.Round(avg, 1),
            MaxChoicesInScene     = max,
            MinChoicesInScene     = min,
            BrokenNextSceneRefs   = [.. brokenRefs]
        };
    }
}

/// <summary>
/// Metrics produced by <see cref="StoryGraphAnalyzer.Analyze"/>.
/// </summary>
public sealed record StoryGraphStats
{
    public int TotalScenes { get; init; }
    public int TotalChoices { get; init; }

    /// <summary>Scenes with no outgoing choices (dead ends / endings).</summary>
    public int EndingScenes { get; init; }

    /// <summary>Scenes (excluding start) with no incoming edges.</summary>
    public int OrphanScenes { get; init; }

    /// <summary>Ending scenes reachable from the start scene.</summary>
    public int ReachableEndings { get; init; }

    /// <summary>Scenes unreachable by following choices from the start scene.</summary>
    public int UnreachableScenes { get; init; }

    /// <summary>Number of unique acyclic paths from start to an ending, capped at 10,000.</summary>
    public int UniquePathCount { get; init; }

    /// <summary>True when the path count hit the 10,000 cap before completion.</summary>
    public bool PathCountExceedsLimit { get; init; }

    public double AvgChoicesPerScene { get; init; }
    public int MaxChoicesInScene { get; init; }
    public int MinChoicesInScene { get; init; }

    /// <summary>
    /// <c>nextScene</c> values that reference scene IDs not found in the story.
    /// </summary>
    public IReadOnlyList<string> BrokenNextSceneRefs { get; init; } = [];
}
