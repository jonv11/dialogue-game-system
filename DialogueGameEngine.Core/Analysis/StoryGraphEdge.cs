namespace DialogueGameEngine.Core.Analysis;

/// <summary>The static source of a graph edge between scenes.</summary>
public enum StoryGraphEdgeKind
{
    ChoiceNextScene,
    ChoiceEffectMoveToScene,
    OnEnterEffectMoveToScene,
    OnExitEffectMoveToScene
}

/// <summary>A statically discoverable transition from one scene to another.</summary>
public sealed record StoryGraphEdge
{
    public required SceneId From { get; init; }
    public required SceneId To { get; init; }
    public required StoryGraphEdgeKind Kind { get; init; }
    public ChoiceId? ChoiceId { get; init; }
}

public static class StoryGraphEdgeCollector
{
    public static IReadOnlyList<StoryGraphEdge> Collect(SceneDefinition scene)
    {
        var edges = new List<StoryGraphEdge>();

        foreach (var choice in scene.Choices)
        {
            if (choice.NextScene is not null)
            {
                edges.Add(new StoryGraphEdge
                {
                    From = scene.Id,
                    To = choice.NextScene.Value,
                    Kind = StoryGraphEdgeKind.ChoiceNextScene,
                    ChoiceId = choice.Id
                });
            }

            foreach (var target in GetMoveTargets(choice.Effects))
            {
                edges.Add(new StoryGraphEdge
                {
                    From = scene.Id,
                    To = target,
                    Kind = StoryGraphEdgeKind.ChoiceEffectMoveToScene,
                    ChoiceId = choice.Id
                });
            }
        }

        foreach (var target in GetMoveTargets(scene.OnEnterEffects))
        {
            edges.Add(new StoryGraphEdge
            {
                From = scene.Id,
                To = target,
                Kind = StoryGraphEdgeKind.OnEnterEffectMoveToScene
            });
        }

        foreach (var target in GetMoveTargets(scene.OnExitEffects))
        {
            edges.Add(new StoryGraphEdge
            {
                From = scene.Id,
                To = target,
                Kind = StoryGraphEdgeKind.OnExitEffectMoveToScene
            });
        }

        return edges;
    }

    public static IEnumerable<SceneId> GetMoveTargets(IEnumerable<IEffect> effects)
    {
        foreach (var effect in effects)
        {
            switch (effect)
            {
                case MoveToSceneEffect move:
                    yield return move.Scene;
                    break;

                case ConditionalEffect conditional:
                    foreach (var target in GetMoveTargets(conditional.Then))
                        yield return target;
                    foreach (var target in GetMoveTargets(conditional.Else))
                        yield return target;
                    break;
            }
        }
    }
}

