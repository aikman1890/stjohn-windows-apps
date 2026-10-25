using System.Globalization;
using StablePilot.Models;
using StablePilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace StablePilot.Views;

public sealed partial class CarePage : Page
{
    private readonly JsonListStore<CareClock> _store = new("clocks.json");
    private readonly JsonListStore<Horse> _horseStore = new("horses.json");
    private List<CareClock> _clocks = new();
    private List<Horse> _horses = new();
    private CareClock? _selected;

    private static readonly Dictionary<string, int> DefaultCycles = new()
    {
        ["Coggins"] = 365, ["Farrier"] = 45, ["Vaccination"] = 365,
        ["Deworming"] = 60, ["Dental"] = 365, ["CVI"] = 30, ["Other"] = 90,
    };

    public CarePage()
    {
        this.InitializeComponent();
        KindBox.ItemsSource = DefaultCycles.Keys.ToList();
        KindBox.SelectionChanged += (_, _) =>
        {
            if (KindBox.SelectedItem is string k && DefaultCycles.TryGetValue(k, out var days))
                CycleBox.Text = days.ToString(CultureInfo.InvariantCulture);
        };
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _horses = (await _horseStore.LoadAsync()).OrderBy(h => h.Name).ToList();
        HorseBox.ItemsSource = _horses.Select(h => h.Name).ToList();
        _clocks = (await _store.LoadAsync()).OrderBy(c => c.NextDueDate).ToList();
        ClockList.ItemsSource = _clocks;
        if (_clocks.Count > 0) ClockList.SelectedIndex = 0;
    }

    private void ClockList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ClockList.SelectedItem as CareClock;
        if (_selected is null) return;
        HorseBox.SelectedItem = _selected.HorseLabel;
        KindBox.SelectedItem = _selected.Kind;
        DetailBox.Text = _selected.Detail;
        CycleBox.Text = _selected.CycleDays.ToString(CultureInfo.InvariantCulture);
        DuePicker.Date = _selected.NextDueDate;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (HorseBox.SelectedIndex < 0) { ErrorText.Text = "Pick the horse."; return; }
        var c = _selected ?? new CareClock();
        var horse = _horses[HorseBox.SelectedIndex];
        c.HorseId = horse.Id; c.HorseLabel = horse.Name;
        c.Kind = KindBox.SelectedItem as string ?? c.Kind;
        c.Detail = DetailBox.Text.Trim();
        if (int.TryParse(CycleBox.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var days) && days > 0) c.CycleDays = days;
        c.NextDueDate = DuePicker.Date ?? c.NextDueDate;
        if (_selected is null) _clocks.Add(c);
        await _store.SaveAsync(_clocks);
        _clocks = _clocks.OrderBy(x => x.NextDueDate).ToList();
        ClockList.ItemsSource = _clocks;
    }

    private async void DoneButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _selected.LastDoneDate = DateTimeOffset.Now;
        _selected.NextDueDate = DateTimeOffset.Now.AddDays(_selected.CycleDays);
        await _store.SaveAsync(_clocks);
        ClockList.ItemsSource = null;
        ClockList.ItemsSource = _clocks;
        ClockList.SelectedItem = _selected;
    }

    private void NewButton_Click(object sender, RoutedEventArgs e)
    {
        _selected = null;
        ClockList.SelectedItem = null;
        DetailBox.Text = string.Empty;
        KindBox.SelectedItem = "Farrier";
        CycleBox.Text = "45";
        DuePicker.Date = DateTimeOffset.Now.AddDays(45);
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _clocks.Remove(_selected);
        await _store.SaveAsync(_clocks);
        _selected = null;
        ClockList.ItemsSource = _clocks;
    }
}
