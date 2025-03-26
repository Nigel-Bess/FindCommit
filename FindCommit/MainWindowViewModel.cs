using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;

namespace FindCommit;

[ObservableObject]
internal partial class MainWindowViewModel
{
    [ObservableProperty]
    private ObservableCollection<Commit> _currentlyVisibleCommits = new();

    public Action<Commit> OpenCommit { get; set; } = _ => { };

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

    [RelayCommand]
    public void InvokeOpenCommit(Commit commit) => OpenCommit(commit);

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
