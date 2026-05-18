namespace DialogueGameEngine.Core;

/// <summary>
/// A scene definition with optional source metadata from persistence.
/// </summary>
public sealed record SceneDocument
{
    public required SceneDefinition Scene { get; init; }

    /// <summary>The file path the scene was loaded from, when known.</summary>
    public string? FilePath { get; init; }
}

