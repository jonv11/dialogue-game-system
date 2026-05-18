namespace DialogueGameEngine.Cli.Tests;

using System.Text.Json;

public class ValidateCommandTests
{
    [Fact]
    public async Task Help_IncludesValidateAndKeyOptions()
    {
        var result = await CliProcess.RunAsync("--help");

        result.ExitCode.Should().Be(0);
        result.StandardOutput.Should().Contain("validate");
        result.StandardOutput.Should().Contain("--warnings-as-errors");
    }

    [Fact]
    public async Task ValidateHelp_IncludesKeyOptions()
    {
        var result = await CliProcess.RunAsync("validate", "--help");

        result.ExitCode.Should().Be(0);
        result.StandardOutput.Should().Contain("--story");
        result.StandardOutput.Should().Contain("--format");
        result.StandardOutput.Should().Contain("--warnings-as-errors");
    }

    [Fact]
    public async Task Validate_ValidStory_ExitsZero()
    {
        using var story = TempStory.Create();
        story.WriteValidMinimalStory();

        var result = await CliProcess.RunAsync("validate", "--story", story.Path);

        result.ExitCode.Should().Be(0);
        result.StandardOutput.Should().Contain("Story is valid");
    }

    [Fact]
    public async Task Validate_InvalidStory_ExitsNonZeroAndPrintsIssue()
    {
        using var story = TempStory.Create();
        story.Write("_initial-state.json", """
            { "currentScene": "start", "attributes": [], "flags": [] }
            """);
        story.Write("start.json", """
            {
              "id": "start",
              "title": "Start",
              "choices": [
                { "id": "go", "text": "Go", "nextScene": "missing" }
              ]
            }
            """);

        var result = await CliProcess.RunAsync("validate", "--story", story.Path);

        result.ExitCode.Should().NotBe(0);
        result.StandardOutput.Should().Contain(StoryValidationCodes.UnknownSceneReference);
        result.StandardOutput.Should().Contain("missing");
    }

    [Fact]
    public async Task Validate_JsonFormat_WritesStructuredResult()
    {
        using var story = TempStory.Create();
        story.WriteValidMinimalStory();

        var result = await CliProcess.RunAsync("validate", "--story", story.Path, "--format", "json");

        result.ExitCode.Should().Be(0);
        using var document = JsonDocument.Parse(result.StandardOutput);
        document.RootElement.GetProperty("isValid").GetBoolean().Should().BeTrue();
        document.RootElement.GetProperty("issues").GetArrayLength().Should().Be(0);
    }

    [Fact]
    public async Task Validate_WarningsAsErrors_ReturnsNonZeroForWarnings()
    {
        using var story = TempStory.Create();
        story.Write("_initial-state.json", """
            { "currentScene": "start", "attributes": [], "flags": [] }
            """);
        story.Write("start.json", """
            { "id": "start", "title": "Start", "choices": [] }
            """);
        story.Write("unused.json", """
            { "id": "unused", "title": "Unused", "choices": [] }
            """);

        var normal = await CliProcess.RunAsync("validate", "--story", story.Path);
        var strict = await CliProcess.RunAsync("validate", "--story", story.Path, "--warnings-as-errors");

        normal.ExitCode.Should().Be(0);
        normal.StandardOutput.Should().Contain(StoryValidationCodes.UnreachableScene);
        strict.ExitCode.Should().NotBe(0);
    }

    [Fact]
    public async Task Validate_MissingStoryPath_FailsClearly()
    {
        var result = await CliProcess.RunAsync("validate");

        result.ExitCode.Should().NotBe(0);
        result.StandardOutput.Should().Contain("Missing required --story");
    }

    [Fact]
    public async Task Stats_InvalidJson_FailsStrictly()
    {
        using var story = TempStory.Create();
        story.Write("bad.json", "{ invalid");

        var result = await CliProcess.RunAsync("stats", "--story", story.Path);

        result.ExitCode.Should().NotBe(0);
        result.StandardOutput.Should().Contain(StoryValidationCodes.InvalidJson);
        result.CombinedOutput.Should().NotContain("Skipping");
    }

    [Fact]
    public async Task Inspect_ValidStory_StillWorks()
    {
        using var story = TempStory.Create();
        story.WriteValidMinimalStory();

        var result = await CliProcess.RunAsync("inspect", "--story", story.Path);

        result.ExitCode.Should().Be(0);
        result.StandardOutput.Should().Contain("DEBUG INSPECT");
        result.StandardOutput.Should().Contain("start");
    }

    [Fact]
    public async Task UnknownCommand_FailsWithUsage()
    {
        var result = await CliProcess.RunAsync("nope");

        result.ExitCode.Should().NotBe(0);
        result.StandardOutput.Should().Contain("Unknown command");
        result.StandardOutput.Should().Contain("USAGE");
    }
}

