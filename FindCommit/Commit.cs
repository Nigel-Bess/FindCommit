
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;

namespace FindCommit;

public partial class Commit
{
    public required string Hash { get; init; }
    public required string Message { get; init; }

    public required string RemoteBaseUrl { get; init; }
    public required DateTime Date { get; init; }

    [RelayCommand]
    public void OpenCommit()
    {
        var link = GenerateCommitLink(Hash, RemoteBaseUrl);
        OpenLink(link);
    }
    private void OpenLink(string link) => Process.Start(new ProcessStartInfo(link) { UseShellExecute = true });

    [RelayCommand(CanExecute = nameof(CanOpenPr))]
    public void OpenPr()
    {
        if (!TryGetPrLinK(out var link)) return;
        OpenLink(link);
    }

    private bool CanOpenPr() => TryGetPrLinK(out _);

    [RelayCommand]
    public void CopyCommitHash() => Clipboard.SetText(Hash);

    // Chat GPT Code
    bool TryGetPrLinK(out string link)
    {
        var match = Regex.Match(Message, @"#(?<num>\d+)");
        if (!match.Success)
        {
            link = string.Empty;
            return false;
        }

        var prNumber = match.Groups["num"].Value;
        link = $"{RemoteBaseUrl}/pull/{prNumber}";
        return true;
    }

    string GenerateCommitLink(string hash, string baseUrl) => $"{baseUrl}/commit/{hash}";

}
