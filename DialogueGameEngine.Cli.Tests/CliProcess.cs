namespace DialogueGameEngine.Cli.Tests;

using System.Diagnostics;

internal static class CliProcess
{
    internal static async Task<CliResult> RunAsync(params string[] args)
    {
        var assembly = typeof(CliApplication).Assembly.Location;
        var startInfo = new ProcessStartInfo("dotnet")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        startInfo.ArgumentList.Add(assembly);
        foreach (var arg in args)
            startInfo.ArgumentList.Add(arg);

        using var process = Process.Start(startInfo)!;
        var stdoutTask = process.StandardOutput.ReadToEndAsync();
        var stderrTask = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        return new CliResult(
            process.ExitCode,
            await stdoutTask,
            await stderrTask);
    }
}

internal sealed record CliResult(int ExitCode, string StandardOutput, string StandardError)
{
    internal string CombinedOutput => StandardOutput + StandardError;
}

