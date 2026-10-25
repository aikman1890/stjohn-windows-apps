using GameVault.Models;
using GameVault.Services;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace GameVault.Views;

public sealed partial class DashboardPage : Page
{
    private readonly JsonListStore<GameItem> _store = new("games.json");

    public DashboardPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var items = await _store.LoadAsync();
        TotalText.Text = items.Count.ToString();
        ExeText.Text = items.Count(g => g.FileKind == "EXE").ToString();
        HtmlText.Text = items.Count(g => g.FileKind == "HTML").ToString();
        ZipText.Text = items.Count(g => g.FileKind == "ZIP").ToString();
        GenreList.ItemsSource = items
            .GroupBy(g => g.Genre)
            .OrderByDescending(g => g.Count())
            .Select(g => $"{g.Key}: {g.Count()} game(s)")
            .ToList();
    }
}
