namespace DialogueGameEngine.Core;

/// <summary>
/// The runtime coordinator of the dialogue system — evaluates conditions, applies effects,
/// and drives the story forward one choice at a time.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="DialogueEngine"/> is stateless. All story state lives in <see cref="GameState"/>,
/// which is passed into each call. This means you can fork, replay, or serialize state
/// independently of the engine instance.
/// </para>
/// <para>
/// <b>Typical usage per game tick:</b>
/// <code>
/// // 1. Find out what the player can do
/// var choices = engine.GetAvailableChoices(state);
///
/// // 2. Display the choices to the player, then:
/// engine.SelectChoice(state, new ChoiceId("confess"));
/// </code>
/// </para>
/// </remarks>
public sealed class DialogueEngine
{
    private readonly IReadOnlyDictionary<SceneId, SceneDefinition> _scenes;

    /// <summary>
    /// Creates a new engine loaded with the given scene definitions.
    /// </summary>
    /// <param name="scenes">
    /// All scene definitions that make up the story. Each scene's <see cref="SceneDefinition.Id"/>
    /// must be unique.
    /// </param>
    public DialogueEngine(IEnumerable<SceneDefinition> scenes)
        : this(scenes.Select(scene => new SceneDocument { Scene = scene }))
    {
    }

    /// <summary>
    /// Creates a new engine loaded with scene definitions and optional file metadata.
    /// </summary>
    public DialogueEngine(IEnumerable<SceneDocument> scenes)
    {
        var documents = scenes.ToList();
        var duplicateIssues = CreateDuplicateSceneIssues(documents);
        if (duplicateIssues.Count > 0)
            throw new StoryValidationException("Duplicate scene IDs were found.", duplicateIssues);

        _scenes = documents.ToDictionary(d => d.Scene.Id, d => d.Scene);
    }

    /// <summary>
    /// Starts a new session by running the current scene's enter lifecycle once.
    /// Calling this again on the same started state is a no-op.
    /// </summary>
    public void Start(GameState state)
    {
        if (state.HasStarted)
            return;

        var scene = GetCurrentScene(state);
        var context = CreateContext(state, scene);

        foreach (var effect in scene.OnEnterEffects)
            effect.Apply(state, context);

        state.MarkStarted();
    }

    /// <summary>
    /// Returns the definition of the scene the player is currently in.
    /// </summary>
    /// <param name="state">The current game state.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if <see cref="GameState.CurrentScene"/> does not match any loaded scene definition.
    /// This usually means the story JSON is missing a file, or a <see cref="ChoiceDefinition.NextScene"/>
    /// references a scene that was never loaded.
    /// </exception>
    public SceneDefinition GetCurrentScene(GameState state)
    {
        if (!_scenes.TryGetValue(state.CurrentScene, out var scene))
            throw new InvalidOperationException($"Scene not found: {state.CurrentScene.Value}");

        return scene;
    }

    /// <summary>
    /// Creates the evaluation context for the state's current scene.
    /// </summary>
    /// <param name="state">The current game state.</param>
    /// <returns>
    /// An <see cref="EvaluationContext"/> populated with the current scene and its active modifiers.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if <see cref="GameState.CurrentScene"/> does not match any loaded scene definition.
    /// </exception>
    public EvaluationContext CreateContext(GameState state)
    {
        var scene = GetCurrentScene(state);
        return CreateContext(state, scene);
    }

    /// <summary>
    /// Returns all choices in the current scene whose conditions are currently satisfied.
    /// </summary>
    /// <param name="state">The current game state.</param>
    /// <returns>
    /// A filtered list of choices the player can legally select right now.
    /// The order matches the order in <see cref="SceneDefinition.Choices"/>.
    /// </returns>
    public IReadOnlyList<ChoiceDefinition> GetAvailableChoices(GameState state)
    {
        var context = CreateContext(state);
        var scene = context.Scene;

        return scene.Choices
            .Where(c => c.IsAvailable(context))
            .ToList();
    }

    /// <summary>
    /// Applies lifecycle effects, the effects of the chosen option, and moves to the next scene.
    /// </summary>
    /// <param name="state">The game state to mutate.</param>
    /// <param name="choiceId">The ID of the choice the player selected.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the choice ID is not found in the current scene,
    /// or if the choice's condition is not currently met.
    /// </exception>
    public void SelectChoice(GameState state, ChoiceId choiceId)
    {
        var context = CreateContext(state);
        var scene = context.Scene;

        var choice = scene.Choices.SingleOrDefault(c => c.Id == choiceId)
            ?? throw new InvalidOperationException($"Choice not found: {choiceId.Value}");

        if (!choice.IsAvailable(context))
            throw new InvalidOperationException($"Choice is not available: {choiceId.Value}");

        foreach (var effect in scene.OnExitEffects)
            effect.Apply(state, context);

        foreach (var effect in choice.Effects)
            effect.Apply(state, context);

        if (choice.NextScene is not null)
            state.MoveTo(choice.NextScene.Value);

        var targetScene = GetCurrentScene(state);
        var targetContext = CreateContext(state, targetScene);
        foreach (var effect in targetScene.OnEnterEffects)
            effect.Apply(state, targetContext);
    }

    private static IReadOnlyList<StoryValidationIssue> CreateDuplicateSceneIssues(
        IReadOnlyList<SceneDocument> documents)
    {
        return documents
            .GroupBy(d => d.Scene.Id.Value, StringComparer.Ordinal)
            .Where(g => g.Count() > 1)
            .Select(g =>
            {
                var paths = g.Select(d => d.FilePath)
                    .Where(p => !string.IsNullOrWhiteSpace(p))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var pathText = paths.Count == 0
                    ? string.Empty
                    : $"{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", paths)}";

                return StoryValidationIssue.Error(
                    StoryValidationCodes.DuplicateSceneId,
                    $"Duplicate scene id '{g.Key}' found.{pathText}",
                    paths.FirstOrDefault(),
                    g.Key);
            })
            .ToList();
    }

    private static EvaluationContext CreateContext(GameState state, SceneDefinition scene)
    {
        // Only CurrentScene modifiers are active here.
        // Permanent and UntilCleared modifiers would need to be stored and retrieved
        // from GameState separately — that tracking layer is not yet implemented.
        var activeModifiers = scene.Modifiers
            .Where(m => m.Duration == ModifierDuration.CurrentScene)
            .ToList();

        return new EvaluationContext
        {
            State = state,
            Scene = scene,
            ActiveModifiers = activeModifiers
        };
    }
}
