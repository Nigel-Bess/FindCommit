
using System.Diagnostics;

namespace FindCommit;

public static class GitFunctionality
{
    // Chat GPT Code
    public static string GetRemoteUrl(string folderPath)
    {
        var startInfo = new ProcessStartInfo("git", "config --get remote.origin.url")
        {
            WorkingDirectory = folderPath,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        using var proc = Process.Start(startInfo)!;
        var remote = (proc.StandardOutput.ReadLine() ?? throw new InvalidOperationException("No remote")).Trim();
        proc.WaitForExit();
        if (remote.StartsWith("git@"))
            remote = "https://" + remote.Substring(4).Replace(':', '/');
        if (remote.EndsWith(".git"))
            remote = remote[..^4];
        return remote;
    }

    // Chat GPT Code
    public static IEnumerable<Commit> GetGitCommits(string? folderPath = null)
    {
        var baseUrl = GetRemoteUrl(folderPath ?? Environment.CurrentDirectory);

        var startInfo = new ProcessStartInfo
        {
            WorkingDirectory = folderPath ?? Environment.CurrentDirectory,
            FileName = "git",
            Arguments = "log --pretty=format:%H|%s",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };
        process.Start();


        string? line;
        while ((line = process.StandardOutput.ReadLine()) != null)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var parts = line.Split('|', 2);
            var hash = parts[0];
            var commitMsg = parts.Length > 1 ? parts[1] : string.Empty;
            yield return new() { RemoteBaseUrl = baseUrl, Hash = hash, Message = commitMsg };
        }

        process.WaitForExit();
    }

}
