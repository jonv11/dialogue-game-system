namespace DialogueGameEngine.Persistence.Tests;

public class JsonInitialStateRepositoryTests
{
    [Fact]
    public void Load_ReadsInitialStateWithoutClampingRawValues()
    {
        using var temp = TempStory.Create();
        temp.Write("_initial-state.json", """
            {
              "currentScene": "start",
              "attributes": [
                {
                  "address": { "scope": "World", "attribute": "Pressure" },
                  "value": 150
                }
              ],
              "flags": ["AlreadyMet"]
            }
            """);
        var repository = CreateInitialRepository(temp.Path);

        var initial = repository.Load();

        initial.Should().NotBeNull();
        initial!.CurrentScene.Should().Be("start");
        initial.Attributes.Should().ContainSingle().Which.Value.Should().Be(150);
        initial.Flags.Should().ContainSingle().Which.Should().Be("AlreadyMet");
    }

    [Fact]
    public void Load_WhenMissing_ReturnsNull()
    {
        using var temp = TempStory.Create();
        var repository = CreateInitialRepository(temp.Path);

        repository.Load().Should().BeNull();
    }

    [Fact]
    public void Load_InvalidJson_ThrowsStructuredIssue()
    {
        using var temp = TempStory.Create();
        var file = temp.Write("_initial-state.json", "{ invalid");
        var repository = CreateInitialRepository(temp.Path);

        var act = () => repository.Load();

        act.Should().Throw<StoryValidationException>()
            .Which.Issues.Should().Contain(i =>
                i.Code == StoryValidationCodes.InvalidInitialState &&
                i.FilePath == file);
    }

    private static JsonInitialStateRepository CreateInitialRepository(string storyDir) =>
        new(
            new PersistenceOptions { StoryDirectory = storyDir, SaveFilePath = string.Empty },
            SerializerOptionsFactory.Create());
}

