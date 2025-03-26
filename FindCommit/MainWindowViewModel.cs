using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows;

namespace FindCommit;

internal partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<Commit> _currentlyVisibleCommits = new();

    public string SearchText
    {
        get => field;
        set
        {
            field = value;
            RecalculateSearchResults();
        }
    } = string.Empty;

    private const int _maxSearchResults = 100;
    public List<Commit> AllCommits
    {
        get => field;
        set
        {
            field = value;
            Task.Run(() => RecalculateSearchResults());
        }
    } = new();

    private void RecalculateSearchResults()
    {
        var currentlyVisibleCommits = CurrentlyVisibleCommits;
        currentlyVisibleCommits.Clear();
        var foundCommits = 0;
        var queryWords = SearchText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        foreach (var commit in AllCommits)
        {
            if (!DoesCommitMatch(commit, queryWords)) continue;
            Application.Current.Dispatcher.Invoke(() => currentlyVisibleCommits.Add(commit));
            foundCommits++;
            if (foundCommits >= _maxSearchResults)
                break;
        }
    }

    private bool DoesCommitMatch(Commit commit, string[] queryWords)
    {
        if (!queryWords.Any()) return true;
        var searchableText = $"{commit.Message} {commit.Hash}";
        return queryWords.All(word => searchableText.Contains(word, StringComparison.OrdinalIgnoreCase));
    }
}
