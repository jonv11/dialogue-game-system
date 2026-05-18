namespace DialogueGameEngine.Core;

using DialogueGameEngine.Core.Analysis;

/// <summary>Validates loaded story definitions independently of any CLI presentation.</summary>
public sealed class StoryValidator
{
    public StoryValidationResult Validate(
        IEnumerable<SceneDocument> sceneDocuments,
        InitialStateDefinition? initialState = null,
        StoryValidationOptions? options = null)
    {
        options ??= new StoryValidationOptions();
        var documents = sceneDocuments.ToList();
        var result = new StoryValidationResult();

        ValidateSceneIdentities(documents, result);

        var sceneIds = documents
            .Select(d => d.Scene.Id)
            .Where(id => !string.IsNullOrWhiteSpace(id.Value))
            .Distinct()
            .ToHashSet();

        foreach (var document in documents)
            ValidateScene(document, result);

        ValidateInitialState(initialState, sceneIds, result);
        ValidateGraph(documents, initialState, options, sceneIds, result);

        return result;
    }

    private static void ValidateSceneIdentities(
        IReadOnlyList<SceneDocument> documents,
        StoryValidationResult result)
    {
        foreach (var document in documents)
        {
            if (string.IsNullOrWhiteSpace(document.Scene.Id.Value))
            {
                result.Add(StoryValidationIssue.Error(
                    StoryValidationCodes.InvalidIdentifier,
                    "Scene id must be non-empty.",
                    document.FilePath,
                    field: "id"));
            }
        }

        foreach (var duplicate in documents
            .Where(d => !string.IsNullOrWhiteSpace(d.Scene.Id.Value))
            .GroupBy(d => d.Scene.Id.Value, StringComparer.Ordinal)
            .Where(g => g.Count() > 1))
        {
            var paths = duplicate
                .Select(d => d.FilePath)
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            var pathText = paths.Count == 0
                ? string.Empty
                : $"{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", paths)}";

            result.Add(StoryValidationIssue.Error(
                StoryValidationCodes.DuplicateSceneId,
                $"Duplicate scene id '{duplicate.Key}' found.{pathText}",
                paths.FirstOrDefault(),
                duplicate.Key));
        }
    }

    private static void ValidateScene(SceneDocument document, StoryValidationResult result)
    {
        var scene = document.Scene;
        var filePath = document.FilePath;
        var sceneId = scene.Id.Value;

        if (string.IsNullOrWhiteSpace(scene.Title))
        {
            result.Add(StoryValidationIssue.Error(
                StoryValidationCodes.MissingRequiredField,
                $"Scene '{sceneId}' title must be non-empty.",
                filePath,
                sceneId,
                field: "title"));
        }

        for (var i = 0; i < scene.Participants.Count; i++)
            ValidateStringId(scene.Participants[i].Value, "participant", result, filePath, sceneId, field: $"participants[{i}]");

        for (var i = 0; i < scene.Ambiance.Count; i++)
        {
            ValidateAttributeAddress(scene.Ambiance[i].Target, result, filePath, sceneId, field: $"ambiance[{i}].target");
            WarnIfOutOfRange(scene.Ambiance[i].Value, result, filePath, sceneId, field: $"ambiance[{i}].value");
        }

        for (var i = 0; i < scene.Modifiers.Count; i++)
        {
            var modifier = scene.Modifiers[i];
            ValidateStringId(modifier.Id.Value, "modifier id", result, filePath, sceneId, field: $"modifiers[{i}].id");
            ValidateAttributeAddress(modifier.Target, result, filePath, sceneId, field: $"modifiers[{i}].target");
            if (!Enum.IsDefined(modifier.Duration))
            {
                result.Add(StoryValidationIssue.Error(
                    StoryValidationCodes.InvalidEffectPayload,
                    $"Scene '{sceneId}', modifiers[{i}]: invalid duration '{modifier.Duration}'.",
                    filePath,
                    sceneId,
                    field: $"modifiers[{i}].duration"));
            }
            if (modifier.Condition is not null)
                ValidateCondition(modifier.Condition, result, filePath, sceneId, null, $"modifiers[{i}].condition");
        }

        ValidateChoices(scene, filePath, result);

        ValidateEffectList(
            scene.OnEnterEffects,
            result,
            filePath,
            sceneId,
            null,
            "onEnterEffects",
            lifecycle: true);
        ValidateEffectList(
            scene.OnExitEffects,
            result,
            filePath,
            sceneId,
            null,
            "onExitEffects",
            lifecycle: true);
    }

    private static void ValidateChoices(SceneDefinition scene, string? filePath, StoryValidationResult result)
    {
        var sceneId = scene.Id.Value;

        foreach (var duplicate in scene.Choices
            .Where(c => !string.IsNullOrWhiteSpace(c.Id.Value))
            .GroupBy(c => c.Id.Value, StringComparer.Ordinal)
            .Where(g => g.Count() > 1))
        {
            result.Add(StoryValidationIssue.Error(
                StoryValidationCodes.DuplicateChoiceId,
                $"Scene '{sceneId}' has duplicate choice id '{duplicate.Key}'.",
                filePath,
                sceneId,
                duplicate.Key));
        }

        for (var i = 0; i < scene.Choices.Count; i++)
        {
            var choice = scene.Choices[i];
            var choiceId = choice.Id.Value;
            ValidateStringId(choiceId, "choice id", result, filePath, sceneId, choiceId, $"choices[{i}].id");

            if (string.IsNullOrWhiteSpace(choice.Text))
            {
                result.Add(StoryValidationIssue.Error(
                    StoryValidationCodes.MissingRequiredField,
                    $"Scene '{sceneId}', choice '{choiceId}': text must be non-empty.",
                    filePath,
                    sceneId,
                    choiceId,
                    $"choices[{i}].text"));
            }

            if (choice.Condition is not null)
                ValidateCondition(choice.Condition, result, filePath, sceneId, choiceId, $"choices[{i}].condition");

            ValidateEffectList(choice.Effects, result, filePath, sceneId, choiceId, $"choices[{i}].effects", lifecycle: false);

            if (choice.NextScene is not null && string.IsNullOrWhiteSpace(choice.NextScene.Value.Value))
            {
                result.Add(StoryValidationIssue.Error(
                    StoryValidationCodes.InvalidIdentifier,
                    $"Scene '{sceneId}', choice '{choiceId}': nextScene must be non-empty.",
                    filePath,
                    sceneId,
                    choiceId,
                    $"choices[{i}].nextScene"));
            }
        }
    }

    private static void ValidateInitialState(
        InitialStateDefinition? initialState,
        HashSet<SceneId> sceneIds,
        StoryValidationResult result)
    {
        if (initialState is null)
            return;

        if (string.IsNullOrWhiteSpace(initialState.CurrentScene))
        {
            result.Add(StoryValidationIssue.Error(
                StoryValidationCodes.InvalidInitialState,
                "_initial-state.json must specify a non-empty currentScene.",
                field: "currentScene"));
        }
        else if (sceneIds.Count > 0 && !sceneIds.Contains(new SceneId(initialState.CurrentScene)))
        {
            result.Add(StoryValidationIssue.Error(
                StoryValidationCodes.MissingStartScene,
                $"Initial state references unknown currentScene '{initialState.CurrentScene}'.",
                sceneId: initialState.CurrentScene,
                field: "currentScene"));
        }

        for (var i = 0; i < initialState.Attributes.Count; i++)
        {
            var attribute = initialState.Attributes[i];
            if (attribute.Address is null)
            {
                result.Add(StoryValidationIssue.Error(
                    StoryValidationCodes.InvalidInitialState,
                    $"_initial-state.json attributes[{i}] is missing address.",
                    field: $"attributes[{i}].address"));
            }
            else
            {
                ValidateAttributeAddress(attribute.Address, result, field: $"attributes[{i}].address");
            }

            if (attribute.Value is null)
            {
                result.Add(StoryValidationIssue.Error(
                    StoryValidationCodes.InvalidInitialState,
                    $"_initial-state.json attributes[{i}] is missing value.",
                    field: $"attributes[{i}].value"));
            }
            else
            {
                WarnIfOutOfRange(attribute.Value.Value, result, field: $"attributes[{i}].value");
            }
        }

        for (var i = 0; i < initialState.Flags.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(initialState.Flags[i]))
            {
                result.Add(StoryValidationIssue.Error(
                    StoryValidationCodes.InvalidInitialState,
                    $"_initial-state.json flags[{i}] must be non-empty.",
                    field: $"flags[{i}]"));
            }
        }
    }

    private static void ValidateGraph(
        IReadOnlyList<SceneDocument> documents,
        InitialStateDefinition? initialState,
        StoryValidationOptions options,
        HashSet<SceneId> sceneIds,
        StoryValidationResult result)
    {
        if (documents.Count == 0)
        {
            result.Add(StoryValidationIssue.Error(
                StoryValidationCodes.MissingStartScene,
                "Story must contain at least one scene."));
            result.Add(StoryValidationIssue.Error(
                StoryValidationCodes.NoPlayablePath,
                "Story has no playable path because it contains no scenes."));
            return;
        }

        var startScene = ResolveStartScene(documents, initialState, options);
        if (string.IsNullOrWhiteSpace(startScene.Value))
        {
            result.Add(StoryValidationIssue.Error(
                StoryValidationCodes.MissingStartScene,
                "Story start scene must be non-empty.",
                field: "start"));
            return;
        }

        if (!sceneIds.Contains(startScene))
        {
            result.Add(StoryValidationIssue.Error(
                StoryValidationCodes.MissingStartScene,
                $"Start scene '{startScene.Value}' does not exist.",
                sceneId: startScene.Value,
                field: "start"));
            return;
        }

        var byId = documents
            .GroupBy(d => d.Scene.Id)
            .Where(g => !string.IsNullOrWhiteSpace(g.Key.Value))
            .ToDictionary(g => g.Key, g => g.First());

        var outEdges = byId.Keys.ToDictionary(id => id, _ => new List<SceneId>());
        var inDegree = byId.Keys.ToDictionary(id => id, _ => 0);

        foreach (var document in documents)
        {
            var scene = document.Scene;
            if (string.IsNullOrWhiteSpace(scene.Id.Value))
                continue;

            foreach (var edge in StoryGraphEdgeCollector.Collect(scene))
            {
                if (string.IsNullOrWhiteSpace(edge.To.Value))
                    continue;

                if (!sceneIds.Contains(edge.To))
                {
                    var location = edge.ChoiceId is null
                        ? $"Scene '{scene.Id.Value}'"
                        : $"Scene '{scene.Id.Value}', choice '{edge.ChoiceId.Value.Value}'";
                    result.Add(StoryValidationIssue.Error(
                        StoryValidationCodes.UnknownSceneReference,
                        $"{location}: unknown scene reference '{edge.To.Value}'.",
                        document.FilePath,
                        scene.Id.Value,
                        edge.ChoiceId?.Value,
                        EdgeField(edge.Kind)));
                    continue;
                }

                outEdges[scene.Id].Add(edge.To);
                inDegree[edge.To] = inDegree.GetValueOrDefault(edge.To) + 1;
            }
        }

        var reachable = new HashSet<SceneId> { startScene };
        var queue = new Queue<SceneId>();
        queue.Enqueue(startScene);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            foreach (var next in outEdges[current])
            {
                if (reachable.Add(next))
                    queue.Enqueue(next);
            }
        }

        foreach (var unreachable in byId.Keys.Where(id => !reachable.Contains(id)).OrderBy(id => id.Value))
        {
            result.Add(StoryValidationIssue.Warning(
                StoryValidationCodes.UnreachableScene,
                $"Scene '{unreachable.Value}' is unreachable from start scene '{startScene.Value}'.",
                byId[unreachable].FilePath,
                unreachable.Value));
        }

        foreach (var orphan in byId.Keys.Where(id => id != startScene && inDegree[id] == 0).OrderBy(id => id.Value))
        {
            result.Add(StoryValidationIssue.Warning(
                StoryValidationCodes.UnreachableScene,
                $"Scene '{orphan.Value}' is an orphan: no static transition references it.",
                byId[orphan].FilePath,
                orphan.Value));
        }

        var endings = byId.Keys.Where(id => outEdges[id].Count == 0).ToList();
        if (endings.Count == 0)
        {
            result.Add(StoryValidationIssue.Warning(
                StoryValidationCodes.NoPlayablePath,
                "Story has no statically identifiable ending scenes."));
        }

        if (!reachable.Any(id => outEdges[id].Count == 0))
        {
            result.Add(StoryValidationIssue.Error(
                StoryValidationCodes.NoPlayablePath,
                $"Story has no playable path from start scene '{startScene.Value}' to an ending."));
        }
    }

    private static SceneId ResolveStartScene(
        IReadOnlyList<SceneDocument> documents,
        InitialStateDefinition? initialState,
        StoryValidationOptions options)
    {
        if (options.StartScene is not null)
            return options.StartScene.Value;

        if (!string.IsNullOrWhiteSpace(initialState?.CurrentScene))
            return new SceneId(initialState.CurrentScene);

        return documents.OrderBy(d => d.Scene.Id.Value).First().Scene.Id;
    }

    private static string EdgeField(StoryGraphEdgeKind kind) => kind switch
    {
        StoryGraphEdgeKind.ChoiceNextScene => "nextScene",
        StoryGraphEdgeKind.ChoiceEffectMoveToScene => "effects",
        StoryGraphEdgeKind.OnEnterEffectMoveToScene => "onEnterEffects",
        StoryGraphEdgeKind.OnExitEffectMoveToScene => "onExitEffects",
        _ => "sceneReference"
    };

    private static void ValidateEffectList(
        IReadOnlyList<IEffect>? effects,
        StoryValidationResult result,
        string? filePath,
        string? sceneId,
        string? choiceId,
        string field,
        bool lifecycle)
    {
        if (effects is null)
        {
            result.Add(StoryValidationIssue.Error(
                lifecycle ? StoryValidationCodes.InvalidLifecycleEffect : StoryValidationCodes.InvalidEffectPayload,
                $"{field} must be an array.",
                filePath,
                sceneId,
                choiceId,
                field));
            return;
        }

        for (var i = 0; i < effects.Count; i++)
            ValidateEffect(effects[i], result, filePath, sceneId, choiceId, $"{field}[{i}]", lifecycle);
    }

    private static void ValidateEffect(
        IEffect? effect,
        StoryValidationResult result,
        string? filePath,
        string? sceneId,
        string? choiceId,
        string field,
        bool lifecycle)
    {
        if (effect is null)
        {
            result.Add(StoryValidationIssue.Error(
                lifecycle ? StoryValidationCodes.InvalidLifecycleEffect : StoryValidationCodes.InvalidEffectPayload,
                $"{field}: effect must not be null.",
                filePath,
                sceneId,
                choiceId,
                field));
            return;
        }

        switch (effect)
        {
            case ChangeAttributeEffect change:
                ValidateAttributeAddress(change.Target, result, filePath, sceneId, choiceId, $"{field}.target");
                break;

            case SetAttributeEffect set:
                ValidateAttributeAddress(set.Target, result, filePath, sceneId, choiceId, $"{field}.target");
                WarnIfOutOfRange(set.Value, result, filePath, sceneId, choiceId, $"{field}.value");
                break;

            case SetFlagEffect setFlag:
                ValidateStringId(setFlag.Flag.Value, "flag", result, filePath, sceneId, choiceId, $"{field}.flag");
                break;

            case ClearFlagEffect clearFlag:
                ValidateStringId(clearFlag.Flag.Value, "flag", result, filePath, sceneId, choiceId, $"{field}.flag");
                break;

            case MoveToSceneEffect move:
                ValidateStringId(move.Scene.Value, "scene", result, filePath, sceneId, choiceId, $"{field}.scene");
                break;

            case ConditionalEffect conditional:
                ValidateCondition(conditional.Condition, result, filePath, sceneId, choiceId, $"{field}.condition");
                ValidateEffectList(conditional.Then, result, filePath, sceneId, choiceId, $"{field}.then", lifecycle);
                ValidateEffectList(conditional.Else, result, filePath, sceneId, choiceId, $"{field}.else", lifecycle);
                break;

            default:
                result.Add(StoryValidationIssue.Error(
                    StoryValidationCodes.UnknownEffectType,
                    $"{field}: unknown effect type '{effect.GetType().Name}'.",
                    filePath,
                    sceneId,
                    choiceId,
                    field));
                break;
        }
    }

    private static void ValidateCondition(
        ICondition? condition,
        StoryValidationResult result,
        string? filePath = null,
        string? sceneId = null,
        string? choiceId = null,
        string field = "condition")
    {
        if (condition is null)
        {
            result.Add(StoryValidationIssue.Error(
                StoryValidationCodes.InvalidConditionPayload,
                $"{field}: condition is required.",
                filePath,
                sceneId,
                choiceId,
                field));
            return;
        }

        switch (condition)
        {
            case AttributeCondition attribute:
                ValidateAttributeAddress(attribute.Target, result, filePath, sceneId, choiceId, $"{field}.target");
                if (!Enum.IsDefined(attribute.Operator))
                {
                    result.Add(StoryValidationIssue.Error(
                        StoryValidationCodes.InvalidConditionPayload,
                        $"{field}: invalid operator '{attribute.Operator}'.",
                        filePath,
                        sceneId,
                        choiceId,
                        $"{field}.operator"));
                }
                WarnIfOutOfRange(attribute.Value, result, filePath, sceneId, choiceId, $"{field}.value");
                break;

            case FlagCondition flag:
                ValidateStringId(flag.Flag.Value, "flag", result, filePath, sceneId, choiceId, $"{field}.flag");
                break;

            case AllCondition all:
                ValidateConditionList(all.Conditions, result, filePath, sceneId, choiceId, $"{field}.conditions");
                break;

            case AnyCondition any:
                ValidateConditionList(any.Conditions, result, filePath, sceneId, choiceId, $"{field}.conditions");
                break;

            case NotCondition not:
                ValidateCondition(not.Condition, result, filePath, sceneId, choiceId, $"{field}.condition");
                break;

            default:
                result.Add(StoryValidationIssue.Error(
                    StoryValidationCodes.UnknownConditionType,
                    $"{field}: unknown condition type '{condition.GetType().Name}'.",
                    filePath,
                    sceneId,
                    choiceId,
                    field));
                break;
        }
    }

    private static void ValidateConditionList(
        IReadOnlyList<ICondition>? conditions,
        StoryValidationResult result,
        string? filePath,
        string? sceneId,
        string? choiceId,
        string field)
    {
        if (conditions is null)
        {
            result.Add(StoryValidationIssue.Error(
                StoryValidationCodes.InvalidConditionPayload,
                $"{field} must be an array.",
                filePath,
                sceneId,
                choiceId,
                field));
            return;
        }

        for (var i = 0; i < conditions.Count; i++)
            ValidateCondition(conditions[i], result, filePath, sceneId, choiceId, $"{field}[{i}]");
    }

    private static void ValidateAttributeAddress(
        AttributeAddress? address,
        StoryValidationResult result,
        string? filePath = null,
        string? sceneId = null,
        string? choiceId = null,
        string field = "address")
    {
        if (address is null)
        {
            result.Add(StoryValidationIssue.Error(
                StoryValidationCodes.InvalidAttributeAddress,
                $"{field}: attribute address is required.",
                filePath,
                sceneId,
                choiceId,
                field));
            return;
        }

        if (!Enum.IsDefined(address.Scope))
        {
            result.Add(StoryValidationIssue.Error(
                StoryValidationCodes.InvalidAttributeAddress,
                $"{field}: unknown attribute scope '{address.Scope}'.",
                filePath,
                sceneId,
                choiceId,
                $"{field}.scope"));
            return;
        }

        ValidateStringId(address.Attribute.Value, "attribute", result, filePath, sceneId, choiceId, $"{field}.attribute");

        switch (address.Scope)
        {
            case AttributeScope.Character:
                ValidateNullableStringId(address.Character?.Value, "character", result, filePath, sceneId, choiceId, $"{field}.character");
                break;

            case AttributeScope.Relation:
                ValidateNullableStringId(address.From?.Value, "from character", result, filePath, sceneId, choiceId, $"{field}.from");
                ValidateNullableStringId(address.To?.Value, "to character", result, filePath, sceneId, choiceId, $"{field}.to");
                break;

            case AttributeScope.Scene:
                ValidateNullableStringId(address.Scene?.Value, "scene", result, filePath, sceneId, choiceId, $"{field}.scene");
                break;

            case AttributeScope.World:
                break;
        }
    }

    private static void ValidateNullableStringId(
        string? value,
        string label,
        StoryValidationResult result,
        string? filePath,
        string? sceneId,
        string? choiceId,
        string field)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            result.Add(StoryValidationIssue.Error(
                StoryValidationCodes.InvalidAttributeAddress,
                $"{field}: {label} is required and must be non-empty.",
                filePath,
                sceneId,
                choiceId,
                field));
        }
    }

    private static void ValidateStringId(
        string? value,
        string label,
        StoryValidationResult result,
        string? filePath,
        string? sceneId = null,
        string? choiceId = null,
        string? field = null)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            result.Add(StoryValidationIssue.Error(
                StoryValidationCodes.InvalidIdentifier,
                $"{label} must be non-empty.",
                filePath,
                sceneId,
                choiceId,
                field));
        }
    }

    private static void WarnIfOutOfRange(
        int value,
        StoryValidationResult result,
        string? filePath = null,
        string? sceneId = null,
        string? choiceId = null,
        string? field = null)
    {
        if (value is < AttributeValue.Min or > AttributeValue.Max)
        {
            result.Add(StoryValidationIssue.Warning(
                StoryValidationCodes.AttributeValueOutOfRange,
                $"{field ?? "attribute value"} is {value}, which will be clamped to {AttributeValue.Min}..{AttributeValue.Max} at runtime.",
                filePath,
                sceneId,
                choiceId,
                field));
        }
    }
}

