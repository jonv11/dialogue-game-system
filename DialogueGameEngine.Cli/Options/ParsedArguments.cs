namespace DialogueGameEngine.Cli.Options;

internal sealed record ParsedArguments(
    string CommandName,
    IReadOnlyDictionary<string, string> Options,
    IReadOnlySet<string> Flags)
{
    internal bool HasFlag(string name) => Flags.Contains(name);

    internal string? GetOption(string name) =>
        Options.TryGetValue(name, out var value) ? value : null;
}

internal sealed record ArgumentParseResult(
    ParsedArguments Arguments,
    bool ShowHelp = false,
    bool ShowVersion = false,
    string? ErrorMessage = null);

