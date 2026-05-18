namespace DialogueGameEngine.Core;

/// <summary>An exception carrying structured story validation or load issues.</summary>
public sealed class StoryValidationException : Exception
{
    public StoryValidationException(string message, IEnumerable<StoryValidationIssue> issues)
        : base(message)
    {
        Issues = issues.ToList();
    }

    public IReadOnlyList<StoryValidationIssue> Issues { get; }
}

