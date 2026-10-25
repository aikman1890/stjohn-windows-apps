using System.Globalization;
using EstatePilot.Models;
using EstatePilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace EstatePilot.Views;

public sealed partial class ServicesPage : Page
{
    private readonly JsonListStore<ServiceClock> _store = new("clocks.json");
    private readonly JsonListStore<Property> _propertyStore = new("properties.json");
    private List<ServiceClock> _clocks = new();
    private List<Property> _properties = new();
    private ServiceClock? _selected;

    public ServicesPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _properties = (await _propertyStore.LoadAsync()).OrderBy(p => p.Name).ToList();
        PropertyBox.ItemsSource = _properties.Select(p => p.Name).ToList();
        _clocks = (await _store.LoadAsync()).OrderBy(c => c.NextDueDate).ToList();
        ClockList.ItemsSource = _clocks;
        if (_clocks.Count > 0) ClockList.SelectedIndex = 0;
    }

    private void ClockList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ClockList.SelectedItem as ServiceClock;
        if (_selected is null) return;
        PropertyBox.SelectedItem = _selected.PropertyLabel;
        TitleBox.Text = _selected.Title;
        DuePicker.Date = _selected.NextDueDate;
        IntervalBox.Text = _selected.IntervalMonths.ToString(CultureInfo.InvariantCulture);
        NoteBox.Text = _selected.Note;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (PropertyBox.SelectedIndex < 0) { ErrorText.Text = "Pick the property."; return; }
        if (string.IsNullOrWhiteSpace(TitleBox.Text)) { ErrorText.Text = "Name the service."; return; }
        var c = _selected ?? new ServiceClock();
        var p = _properties[PropertyBox.SelectedIndex];
        c.PropertyId = p.Id; c.PropertyLabel = p.Name;
        c.Title = TitleBox.Text.Trim();
        c.NextDueDate = DuePicker.Date ?? c.NextDueDate;
        if (int.TryParse(IntervalBox.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var months) && months > 0) c.IntervalMonths = months;
        c.Note = NoteBox.Text.Trim();
        if (_selected is null) _clocks.Add(c);
        await _store.SaveAsync(_clocks);
        _clocks = _clocks.OrderBy(x => x.NextDueDate).ToList();
        ClockList.ItemsSource = _clocks;
    }

    private async void DoneButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _selected.LastDoneDate = DateTimeOffset.Now;
        _selected.NextDueDate = DateTimeOffset.Now.AddMonths(_selected.IntervalMonths);
        await _store.SaveAsync(_clocks);
        ClockList.ItemsSource = null;
        ClockList.ItemsSource = _clocks;
        ClockList.SelectedItem = _selected;
    }

    private void NewButton_Click(object sender, RoutedEventArgs e)
    {
        _selected = null;
        ClockList.SelectedItem = null;
        TitleBox.Text = NoteBox.Text = string.Empty;
        IntervalBox.Text = "12";
        DuePicker.Date = DateTimeOffset.Now.AddMonths(6);
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
