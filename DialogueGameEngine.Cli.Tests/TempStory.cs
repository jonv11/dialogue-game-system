namespace DialogueGameEngine.Cli.Tests;

internal sealed class TempStory : IDisposable
{
    private TempStory(string path)
    {
        Path = path;
        Directory.CreateDirectory(path);
    }

    internal string Path { get; }

    internal static TempStory Create() =>
        new(System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"dialogue-cli-story-{Guid.NewGuid():N}"));

    internal string Write(string relativePath, string contents)
    {
        var fullPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(Path, relativePath));
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(fullPath)!);
        File.WriteAllText(fullPath, contents);
        return fullPath;
    }

    internal void WriteValidMinimalStory()
    {
        Write("_initial-state.json", """
            { "currentScene": "start", "attributes": [], "flags": [] }
            """);
        Write("start.json", """
            {
              "id": "start",
              "title": "Start",
              "choices": []
            }
            """);
    }

    public void Dispose()
    {
        if (Directory.Exists(Path))
            Directory.Delete(Path, recursive: true);
    }
}

