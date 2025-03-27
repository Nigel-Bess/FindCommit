
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
        var baseDir = folderPath ?? Environment.CurrentDirectory;
        var startInfo = new ProcessStartInfo("git", "log --pretty=format:%H|%cI|%s")
        {
            WorkingDirectory = baseDir,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo)!;
        string? line;
        var remoteBaseUrl = GetRemoteUrl(baseDir);
        while ((line = process.StandardOutput.ReadLine()) != null)
        {
            var parts = line.Split('|', 3);
            yield return new Commit
            {
                RemoteBaseUrl = remoteBaseUrl,
                Hash = parts[0],
                Date = DateTimeOffset.Parse(parts[1]).ToLocalTime().DateTime,
                Message = parts.Length > 2 ? parts[2] : string.Empty
            };
        }
        process.WaitForExit();
    }

}
