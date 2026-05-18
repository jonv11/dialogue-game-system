namespace DialogueGameEngine.Cli.Options;

internal static class ArgumentParser
{
    internal static ArgumentParseResult Parse(string[] args, IEnumerable<string> commandNames)
    {
        var knownCommands = commandNames.ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (args.Length == 0)
            return ParseCommand("play", []);

        if (args is ["--help"] or ["-h"])
            return new ArgumentParseResult(new ParsedArguments("play", new Dictionary<string, string>(), new HashSet<string>()), ShowHelp: true);

        if (args is ["--version"] or ["-v"])
            return new ArgumentParseResult(new ParsedArguments("play", new Dictionary<string, string>(), new HashSet<string>()), ShowVersion: true);

        if (knownCommands.Contains(args[0]))
            return ParseCommand(args[0], args[1..]);

        if (!args[0].StartsWith("-", StringComparison.Ordinal))
        {
            return new ArgumentParseResult(
                new ParsedArguments(args[0], new Dictionary<string, string>(), new HashSet<string>()),
                ErrorMessage: $"Unknown command '{args[0]}'.");
        }

        return ParseCommand("play", args);
    }

    private static ArgumentParseResult ParseCommand(string commandName, string[] args)
    {
        var options = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var flags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < args.Length; i++)
        {
            var token = args[i];
            if (!token.StartsWith("-", StringComparison.Ordinal))
                continue;

            if (token is "--help" or "-h")
            {
                flags.Add("--help");
                continue;
            }

            if (i + 1 < args.Length && !args[i + 1].StartsWith("-", StringComparison.Ordinal))
                options[token] = args[++i];
            else
                flags.Add(token);
        }

        return new ArgumentParseResult(new ParsedArguments(commandName, options, flags));
    }
}

