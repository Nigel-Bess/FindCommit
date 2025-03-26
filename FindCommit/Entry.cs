using System.Diagnostics;

namespace FindCommit;

public static class Entry
{
    static readonly string? path = null;// put a path to your git repository here for debugging (program is intended to be run from a cli from your repo)
    public static void Start()
    {
        var allCommits = GetGitCommits(path).ToList();
        var vm = new MainWindowViewModel();
        vm.AllCommits = allCommits;
        vm.OpenCommit = OpenCommit;
        var window = new MainWindow { DataContext = vm };
        window.Show();
    }

    static IEnumerable<Commit> GetGitCommits(string? path = null)
    {
        var startInfo = new ProcessStartInfo
        {
            WorkingDirectory = path ?? Environment.CurrentDirectory,
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
            yield return new Commit(parts[0], parts.Length > 1 ? parts[1] : string.Empty);
        }

        process.WaitForExit();
    }

    private static void OpenCommit(Commit commit)
    {
        var link = GetCommitLink(commit.Hash, path);
        Process.Start(new ProcessStartInfo(link) { UseShellExecute = true });
    }

    static string GetCommitLink(string hash, string? repoPath = null)
    {
        var startInfo = new ProcessStartInfo("git", "config --get remote.origin.url")
        {
            WorkingDirectory = repoPath ?? Environment.CurrentDirectory,
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

        return $"{remote}/commit/{hash}";
    }
}
