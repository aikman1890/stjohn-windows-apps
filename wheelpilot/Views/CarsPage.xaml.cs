using System.Globalization;
using WheelPilot.Models;
using WheelPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace WheelPilot.Views;

public sealed partial class CarsPage : Page
{
    private readonly JsonListStore<Car> _carStore = new("cars.json");
    private readonly JsonListStore<MaintenanceEntry> _maintStore = new("maintenance.json");
    private readonly JsonListStore<Instructor> _instStore = new("instructors.json");
    private List<Car> _cars = new();
    private List<Instructor> _instructors = new();
    private Car? _car;
    private Instructor? _inst;

    public CarsPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _cars = (await _carStore.LoadAsync()).OrderBy(c => c.Name).ToList();
        CarList.ItemsSource = _cars;
        _instructors = (await _instStore.LoadAsync()).OrderBy(i => i.Name).ToList();
        InstructorList.ItemsSource = _instructors
            .Select(i => $"{i.Name} - {i.Phone}{(i.Active ? string.Empty : " - INACTIVE")}").ToList();
        await RefreshMaintAsync();
        if (_cars.Count > 0) CarList.SelectedIndex = 0;
    }

    private async Task RefreshMaintAsync()
    {
        var maint = await _maintStore.LoadAsync();
        MaintList.ItemsSource = maint
            .Where(m => _car is null || m.CarId == _car.Id)
            .OrderByDescending(m => m.Date).Select(m => m.DisplayLine).ToList();
    }

    private void CarList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _car = CarList.SelectedItem as Car;
        if (_car is null) return;
        CarNameBox.Text = _car.Name;
        PlateBox.Text = _car.PlateText;
        InspPicker.Date = _car.InspectionExpiry;
        RegPicker.Date = _car.RegistrationExpiry;
        InsPicker.Date = _car.InsuranceExpiry;
        OosCheck.IsChecked = _car.OutOfService;
        OosNoteBox.Text = _car.OutOfServiceNote;
        _ = RefreshMaintAsync();
    }

    private async void SaveCarButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(CarNameBox.Text)) { ErrorText.Text = "A car label is required."; return; }
        var c = _car ?? new Car();
        c.Name = CarNameBox.Text.Trim();
        c.PlateText = PlateBox.Text.Trim();
        c.InspectionExpiry = InspPicker.Date;
        c.RegistrationExpiry = RegPicker.Date;
        c.InsuranceExpiry = InsPicker.Date;
        c.OutOfService = OosCheck.IsChecked == true;
        c.OutOfServiceNote = OosNoteBox.Text.Trim();
        if (_car is null) _cars.Add(c);
        await _carStore.SaveAsync(_cars);
        _cars = _cars.OrderBy(x => x.Name).ToList();
        CarList.ItemsSource = _cars;
        CarList.SelectedItem = _cars.FirstOrDefault(x => x.Id == c.Id);
    }

    private void NewCarButton_Click(object sender, RoutedEventArgs e)
    {
        _car = null;
        CarList.SelectedItem = null;
        CarNameBox.Text = PlateBox.Text = OosNoteBox.Text = string.Empty;
        InspPicker.Date = RegPicker.Date = InsPicker.Date = null;
        OosCheck.IsChecked = false;
    }

    private async void DeleteCarButton_Click(object sender, RoutedEventArgs e)
    {
        if (_car is null) return;
        _cars.Remove(_car);
        await _carStore.SaveAsync(_cars);
        _car = null;
        CarList.ItemsSource = _cars;
    }

    private async void MaintButton_Click(object sender, RoutedEventArgs e)
    {
        if (_car is null) { ErrorText.Text = "Select a car first."; return; }
        if (string.IsNullOrWhiteSpace(WorkBox.Text)) { ErrorText.Text = "Describe the work."; return; }
        var maint = await _maintStore.LoadAsync();
        maint.Add(new MaintenanceEntry
        {
            CarId = _car.Id, CarLabel = _car.Name, Work = WorkBox.Text.Trim(),
            Cost = decimal.TryParse(CostBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var c) ? c : 0m,
            Date = DateTimeOffset.Now,
        });
        await _maintStore.SaveAsync(maint);
        WorkBox.Text = CostBox.Text = string.Empty;
        await RefreshMaintAsync();
    }

    private void InstructorList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var idx = InstructorList.SelectedIndex;
        _inst = idx >= 0 && idx < _instructors.Count ? _instructors[idx] : null;
        if (_inst is null) return;
        InstNameBox.Text = _inst.Name;
        InstPhoneBox.Text = _inst.Phone;
        InstLicenseBox.Text = _inst.LicenseNote;
        InstActiveCheck.IsChecked = _inst.Active;
    }

    private async void SaveInstButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(InstNameBox.Text)) { ErrorText.Text = "An instructor name is required."; return; }
        var i = _inst ?? new Instructor();
        i.Name = InstNameBox.Text.Trim();
        i.Phone = InstPhoneBox.Text.Trim();
        i.LicenseNote = InstLicenseBox.Text.Trim();
        i.Active = InstActiveCheck.IsChecked == true;
        if (_inst is null) _instructors.Add(i);
        await _instStore.SaveAsync(_instructors);
        _instructors = _instructors.OrderBy(x => x.Name).ToList();
        InstructorList.ItemsSource = _instructors
            .Select(x => $"{x.Name} - {x.Phone}{(x.Active ? string.Empty : " - INACTIVE")}").ToList();
    }

    private void NewInstButton_Click(object sender, RoutedEventArgs e)
    {
        _inst = null;
        InstructorList.SelectedItem = null;
        InstNameBox.Text = InstPhoneBox.Text = InstLicenseBox.Text = string.Empty;
        InstActiveCheck.IsChecked = true;
    }
}
