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
    /// must be unique — duplicates will cause one to overwrite the other silently.
    /// </param>
    public DialogueEngine(IEnumerable<SceneDefinition> scenes)
    {
        _scenes = scenes.ToDictionary(s => s.Id);
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
    /// Returns all choices in the current scene whose conditions are currently satisfied.
    /// </summary>
    /// <param name="state">The current game state.</param>
    /// <returns>
    /// A filtered list of choices the player can legally select right now.
    /// The order matches the order in <see cref="SceneDefinition.Choices"/>.
    /// </returns>
    public IReadOnlyList<ChoiceDefinition> GetAvailableChoices(GameState state)
    {
        var scene = GetCurrentScene(state);
        var context = CreateContext(state, scene);

        return scene.Choices
            .Where(c => c.IsAvailable(context))
            .ToList();
    }

    /// <summary>
    /// Applies the effects of the chosen option and moves to the next scene.
    /// </summary>
    /// <param name="state">The game state to mutate.</param>
    /// <param name="choiceId">The ID of the choice the player selected.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the choice ID is not found in the current scene,
    /// or if the choice's condition is not currently met.
    /// </exception>
    public void SelectChoice(GameState state, ChoiceId choiceId)
    {
        var scene = GetCurrentScene(state);
        var context = CreateContext(state, scene);

        var choice = scene.Choices.SingleOrDefault(c => c.Id == choiceId)
            ?? throw new InvalidOperationException($"Choice not found: {choiceId.Value}");

        if (!choice.IsAvailable(context))
            throw new InvalidOperationException($"Choice is not available: {choiceId.Value}");

        foreach (var effect in choice.Effects)
            effect.Apply(state, context);

        if (choice.NextScene is not null)
            state.MoveTo(choice.NextScene.Value);
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
