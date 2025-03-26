using System.Diagnostics;

namespace FindCommit;

public static class Entry
{
    public static void Start()
    {
        string folderPath = null;
        if (Debugger.IsAttached) folderPath = "C:\\Dev\\Fulfil.NET";// put whatever path you want here for debugging. (path will populate automatically if running from CLI)
        var commits = GitFunctionality.GetGitCommits(folderPath).ToList();
        var vm = new MainWindowViewModel() { AllCommits = commits };
        var window = new MainWindow { DataContext = vm };
        window.Show();
    }
}
