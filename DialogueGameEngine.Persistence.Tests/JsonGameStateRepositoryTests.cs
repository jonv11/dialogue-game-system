namespace DialogueGameEngine.Persistence.Tests;

public class JsonGameStateRepositoryTests
{
    [Fact]
    public void Load_MissingHasStarted_DefaultsToTrueForSaveFiles()
    {
        using var temp = TempStory.Create();
        var save = temp.Write("save.json", """
            {
              "currentScene": "start",
              "attributes": [],
              "flags": []
            }
            """);
        var repository = new JsonGameStateRepository(
            new PersistenceOptions { StoryDirectory = temp.Path, SaveFilePath = save },
            SerializerOptionsFactory.Create());

        var state = repository.Load();

        state.Should().NotBeNull();
        state!.HasStarted.Should().BeTrue();
    }

    [Fact]
    public void Load_CanDefaultHasStartedToFalseForInitialStateStyleLoading()
    {
        using var temp = TempStory.Create();
        var save = temp.Write("initial.json", """
            {
              "currentScene": "start",
              "attributes": [],
              "flags": []
            }
            """);
        var repository = new JsonGameStateRepository(
            new PersistenceOptions { StoryDirectory = temp.Path, SaveFilePath = save },
            SerializerOptionsFactory.Create());

        var state = repository.Load(hasStartedDefault: false);

        state.Should().NotBeNull();
        state!.HasStarted.Should().BeFalse();
    }
}

