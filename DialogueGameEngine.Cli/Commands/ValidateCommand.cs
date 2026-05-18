namespace DialogueGameEngine.Cli.Commands;

using DialogueGameEngine.Cli.Display;
using DialogueGameEngine.Cli.Options;
using DialogueGameEngine.Core;

internal sealed class ValidateCommand : ICliCommand
{
    public int Execute(ParsedArguments args, CommandContext context)
    {
        if (args.HasFlag("--help"))
        {
            CliHelp.PrintValidate();
            return 0;
        }

        var format = args.GetOption("--format") ?? "text";
        if (!format.Equals("text", StringComparison.OrdinalIgnoreCase) &&
            !format.Equals("json", StringComparison.OrdinalIgnoreCase))
        {
            var invalidFormat = new StoryValidationResult();
            invalidFormat.Add(StoryValidationIssue.Error(
                StoryValidationCodes.InvalidIdentifier,
                $"Unknown validate format '{format}'. Use 'text' or 'json'.",
                field: "format"));
            Render(invalidFormat, format);
            return 1;
        }

        var warningsAsErrors = args.HasFlag("--warnings-as-errors");
        var result = new StoryValidationResult();
        var storyDir = args.GetOption("--story");
        if (string.IsNullOrWhiteSpace(storyDir))
        {
            result.Add(StoryValidationIssue.Error(
                StoryValidationCodes.StoryPathNotFound,
                "Missing required --story <dir> argument.",
                field: "story"));
            Render(result, format);
            if (format.Equals("text", StringComparison.OrdinalIgnoreCase))
                CliHelp.PrintUsage("dialogue-engine validate --story <dir> [--start <scene-id>] [--format text|json] [--warnings-as-errors]");
            return 1;
        }

        storyDir = Path.GetFullPath(storyDir);
        if (!Directory.Exists(storyDir))
        {
            result.Add(StoryValidationIssue.Error(
                StoryValidationCodes.StoryPathNotFound,
                $"Story directory not found: '{storyDir}'.",
                storyDir));
            Render(result, format);
            return 1;
        }

        try
        {
            var documents = CommandHelpers.LoadSceneDocuments(storyDir, context, allowEmpty: true);
            var initialState = CommandHelpers.LoadInitialState(storyDir, context);
            result.AddRange(CommandHelpers.ValidateStory(
                documents,
                initialState,
                args.GetOption("--start"),
                warningsAsErrors).Issues);
        }
        catch (StoryValidationException ex)
        {
            result.AddRange(ex.Issues);
        }

        Render(result, format);
        return CommandHelpers.HasFailingIssues(result, warningsAsErrors) ? 1 : 0;
    }

    private static void Render(StoryValidationResult result, string format)
    {
        if (format.Equals("json", StringComparison.OrdinalIgnoreCase))
            ValidationResultRenderer.RenderJson(result);
        else
            ValidationResultRenderer.RenderText(result);
    }
}
