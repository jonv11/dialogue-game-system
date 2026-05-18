namespace DialogueGameEngine.Core;

/// <summary>Structured validation output for a story.</summary>
public sealed class StoryValidationResult
{
    private readonly List<StoryValidationIssue> _issues = [];

    public IReadOnlyList<StoryValidationIssue> Issues => _issues;
    public bool HasErrors => _issues.Any(i => i.Severity == StoryValidationSeverity.Error);
    public bool HasWarnings => _issues.Any(i => i.Severity == StoryValidationSeverity.Warning);
    public bool IsValid => !HasErrors;

    public void Add(StoryValidationIssue issue) => _issues.Add(issue);

    public void AddRange(IEnumerable<StoryValidationIssue> issues) => _issues.AddRange(issues);
}

