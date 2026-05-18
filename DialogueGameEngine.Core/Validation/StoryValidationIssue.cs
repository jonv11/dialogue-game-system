namespace DialogueGameEngine.Core;

/// <summary>A structured, actionable issue found while loading or validating a story.</summary>
public sealed record StoryValidationIssue
{
    public required StoryValidationSeverity Severity { get; init; }
    public required string Code { get; init; }
    public required string Message { get; init; }
    public string? FilePath { get; init; }
    public string? SceneId { get; init; }
    public string? ChoiceId { get; init; }
    public string? Field { get; init; }

    public static StoryValidationIssue Error(
        string code,
        string message,
        string? filePath = null,
        string? sceneId = null,
        string? choiceId = null,
        string? field = null) =>
        new()
        {
            Severity = StoryValidationSeverity.Error,
            Code = code,
            Message = message,
            FilePath = filePath,
            SceneId = sceneId,
            ChoiceId = choiceId,
            Field = field
        };

    public static StoryValidationIssue Warning(
        string code,
        string message,
        string? filePath = null,
        string? sceneId = null,
        string? choiceId = null,
        string? field = null) =>
        new()
        {
            Severity = StoryValidationSeverity.Warning,
            Code = code,
            Message = message,
            FilePath = filePath,
            SceneId = sceneId,
            ChoiceId = choiceId,
            Field = field
        };
}

