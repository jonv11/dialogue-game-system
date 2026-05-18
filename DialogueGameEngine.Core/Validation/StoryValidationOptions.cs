namespace DialogueGameEngine.Core;

/// <summary>Options controlling static story validation.</summary>
public sealed record StoryValidationOptions
{
    /// <summary>Explicit start scene to use for graph validation. If omitted, initial state or first scene is used.</summary>
    public SceneId? StartScene { get; init; }

    /// <summary>Used by callers when deciding whether warnings should fail their command.</summary>
    public bool WarningsAsErrors { get; init; }
}

