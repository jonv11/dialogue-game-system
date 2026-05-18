namespace DialogueGameEngine.Persistence.Tests;

internal sealed class TempStory : IDisposable
{
    private TempStory(string path)
    {
        Path = path;
        Directory.CreateDirectory(path);
    }

    internal string Path { get; }

    internal static TempStory Create() =>
        new(System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"dialogue-story-{Guid.NewGuid():N}"));

    internal string Write(string relativePath, string contents)
    {
        var fullPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(Path, relativePath));
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(fullPath)!);
        File.WriteAllText(fullPath, contents);
        return fullPath;
    }

    public void Dispose()
    {
        if (Directory.Exists(Path))
            Directory.Delete(Path, recursive: true);
    }
}
