namespace DialogueGameEngine.Persistence.Tests;

public class JsonSceneRepositoryTests
{
    [Fact]
    public void LoadAllDocuments_InvalidJson_ThrowsWithFilePath()
    {
        using var temp = TempStory.Create();
        var badFile = temp.Write("bad.json", "{ invalid");
        var repository = CreateSceneRepository(temp.Path);

        var act = () => repository.LoadAllDocuments();

        act.Should().Throw<StoryValidationException>()
            .Which.Issues.Should().Contain(i =>
                i.Code == StoryValidationCodes.InvalidJson &&
                i.FilePath == badFile);
    }

    [Fact]
    public void LoadAllDocuments_DoesNotWriteToConsoleErrorWhenInvalid()
    {
        using var temp = TempStory.Create();
        temp.Write("bad.json", "{ invalid");
        var repository = CreateSceneRepository(temp.Path);
        using var error = new StringWriter();
        var originalError = Console.Error;
        Console.SetError(error);

        try
        {
            var act = () => repository.LoadAllDocuments();

            act.Should().Throw<StoryValidationException>();
            error.ToString().Should().BeEmpty();
        }
        finally
        {
            Console.SetError(originalError);
        }
    }

    [Fact]
    public void LoadAllDocuments_LoadsScenesRecursivelyAndSkipsUnderscoreFiles()
    {
        using var temp = TempStory.Create();
        temp.Write("act1/scene_a.json", SceneJson("a"));
        temp.Write("act2/scene_b.json", SceneJson("b"));
        temp.Write("_initial-state.json", """
            { "currentScene": "a", "attributes": [], "flags": [] }
            """);
        var repository = CreateSceneRepository(temp.Path);

        var documents = repository.LoadAllDocuments();

        documents.Select(d => d.Scene.Id.Value).Should().BeEquivalentTo("a", "b");
        documents.Should().OnlyContain(d => d.FilePath != null && !Path.GetFileName(d.FilePath).StartsWith('_'));
    }

    [Fact]
    public void LoadAllDocuments_PreservesDuplicateSceneFilePaths()
    {
        using var temp = TempStory.Create();
        var first = temp.Write("a.json", SceneJson("same"));
        var second = temp.Write("nested/b.json", SceneJson("same"));
        var repository = CreateSceneRepository(temp.Path);

        var documents = repository.LoadAllDocuments();

        documents.Should().HaveCount(2);
        documents.Select(d => d.FilePath).Should().BeEquivalentTo(first, second);
    }

    [Fact]
    public void LoadAll_MissingRequiredField_ThrowsStructuredIssue()
    {
        using var temp = TempStory.Create();
        temp.Write("missing-title.json", """
            { "id": "start", "choices": [] }
            """);
        var repository = CreateSceneRepository(temp.Path);

        var act = () => repository.LoadAllDocuments();

        act.Should().Throw<StoryValidationException>()
            .Which.Issues.Should().Contain(i => i.Code == StoryValidationCodes.MissingRequiredField);
    }

    private static JsonSceneRepository CreateSceneRepository(string storyDir) =>
        new(
            new PersistenceOptions { StoryDirectory = storyDir, SaveFilePath = string.Empty },
            SerializerOptionsFactory.Create());

    private static string SceneJson(string id) =>
        $$"""
        {
          "id": "{{id}}",
          "title": "Scene {{id}}",
          "choices": []
        }
        """;
}
