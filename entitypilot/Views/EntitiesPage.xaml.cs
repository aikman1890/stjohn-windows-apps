using EntityPilot.Models;
using EntityPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace EntityPilot.Views;

public sealed partial class EntitiesPage : Page
{
    private readonly JsonListStore<EntityRecord> _store = new("entities.json");
    private readonly JsonListStore<AgentMailEntry> _mailStore = new("agentmail.json");
    private readonly JsonListStore<Obligation> _obligationStore = new("obligations.json");
    private List<EntityRecord> _entities = new();
    private EntityRecord? _selected;
    private bool _einRevealed;

    public EntitiesPage()
    {
        this.InitializeComponent();
        TypeBox.ItemsSource = new[] { "LLC", "S-corp", "C-corp", "LP", "PLLC", "Other" };
        StatusBox.ItemsSource = new[] { "Active", "Inactive", "Dissolved", "Unknown" };
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e) => await ReloadAsync();

    private async Task ReloadAsync(string? keepId = null)
    {
        _entities = (await _store.LoadAsync()).OrderBy(x => x.LegalName).ToList();
        EntityList.ItemsSource = _entities;
        if (keepId is not null)
            EntityList.SelectedItem = _entities.FirstOrDefault(x => x.Id == keepId);
        else if (_entities.Count > 0)
            EntityList.SelectedIndex = 0;
    }

    private async void EntityList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = EntityList.SelectedItem as EntityRecord;
        if (_selected is null) return;
        NameBox.Text = _selected.LegalName;
        TypeBox.SelectedItem = _selected.Type;
        StateBox.Text = _selected.State;
        FormedPicker.Date = _selected.FormationDate;
        FileBox.Text = _selected.FileNumber;
        FyeBox.Text = _selected.FiscalYearEnd;
        EinBox.Text = _selected.Ein;
        _einRevealed = false;
        EinMaskedText.Text = _selected.EinMasked;
        EinBox.Visibility = Visibility.Collapsed;
        StatusBox.SelectedItem = _selected.Status;
        ConfirmedPicker.Date = _selected.StatusConfirmedDate;
        NotesBox.Text = _selected.Notes;
        AgentBox.Text = _selected.RegisteredAgentName;
        AgentAddressBox.Text = _selected.AgentAddress;
        AgentFeeBox.Text = _selected.AgentFeeNote;
        AgentRenewalPicker.Date = _selected.AgentRenewalDate;
        PortalBox.Text = _selected.PortalReference;
        ErrorText.Text = string.Empty;

        var obligations = await _obligationStore.LoadAsync();
        var mine = obligations.Where(o => o.EntityId == _selected.Id).ToList();
        var filingsOk = !mine.Any(o => o.DaysUntil < 0);
        var agentOk = _selected.AgentDays is not < 0;
        StandingText.Text = $"Standing checklist (from your records): filings {(filingsOk ? "current" : "OVERDUE ITEMS")} - agent {(agentOk ? "current" : "RENEWAL OVERDUE")} - status {_selected.Status}" +
            (_selected.StatusConfirmedDate is { } c ? $" as confirmed {c:MMM d, yyyy}" : " (never confirmed)");

        var mail = await _mailStore.LoadAsync();
        MailList.ItemsSource = mail.Where(m => m.EntityId == _selected.Id)
            .OrderByDescending(m => m.Date).Select(m => m.DisplayLine).ToList();
    }

    private void RevealButton_Click(object sender, RoutedEventArgs e)
    {
        _einRevealed = !_einRevealed;
        EinBox.Visibility = _einRevealed ? Visibility.Visible : Visibility.Collapsed;
        if (_selected is not null)
            EinMaskedText.Text = _einRevealed ? _selected.Ein : _selected.EinMasked;
    }

    private void ReadForm(EntityRecord x)
    {
        x.LegalName = NameBox.Text.Trim();
        x.Type = TypeBox.SelectedItem as string ?? x.Type;
        x.State = StateBox.Text.Trim().ToUpperInvariant();
        x.FormationDate = FormedPicker.Date ?? x.FormationDate;
        x.FileNumber = FileBox.Text.Trim();
        x.FiscalYearEnd = FyeBox.Text.Trim();
        x.Ein = EinBox.Text.Trim();
        x.Status = StatusBox.SelectedItem as string ?? x.Status;
        x.StatusConfirmedDate = ConfirmedPicker.Date;
        x.Notes = NotesBox.Text.Trim();
        x.RegisteredAgentName = AgentBox.Text.Trim();
        x.AgentAddress = AgentAddressBox.Text.Trim();
        x.AgentFeeNote = AgentFeeBox.Text.Trim();
        x.AgentRenewalDate = AgentRenewalPicker.Date;
        x.PortalReference = PortalBox.Text.Trim();
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameBox.Text)) { ErrorText.Text = "A legal name is required."; return; }
        var x = _selected ?? new EntityRecord();
        ReadForm(x);
        if (_selected is null) _entities.Add(x);
        await _store.SaveAsync(_entities);
        await ReloadAsync(x.Id);
    }

    private async void ConfirmButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _selected.StatusConfirmedDate = DateTimeOffset.Now;
        await _store.SaveAsync(_entities);
        await ReloadAsync(_selected.Id);
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _entities.Remove(_selected);
        await _store.SaveAsync(_entities);
        _selected = null;
        await ReloadAsync();
    }

    private void NewButton_Click(object sender, RoutedEventArgs e)
    {
        _selected = null;
        EntityList.SelectedItem = null;
        NameBox.Text = StateBox.Text = FileBox.Text = EinBox.Text = NotesBox.Text =
            AgentBox.Text = AgentAddressBox.Text = AgentFeeBox.Text = PortalBox.Text = string.Empty;
        FyeBox.Text = "12-31";
        TypeBox.SelectedItem = "LLC";
        StatusBox.SelectedItem = "Unknown";
        FormedPicker.Date = DateTimeOffset.Now;
        ConfirmedPicker.Date = null;
        AgentRenewalPicker.Date = null;
        EinMaskedText.Text = "(not recorded)";
        StandingText.Text = string.Empty;
        MailList.ItemsSource = null;
    }

    private async void MailButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) { ErrorText.Text = "Select an entity first."; return; }
        if (string.IsNullOrWhiteSpace(ArrivedBox.Text)) { ErrorText.Text = "Note what arrived."; return; }
        var mail = await _mailStore.LoadAsync();
        mail.Add(new AgentMailEntry
        {
            EntityId = _selected.Id, EntityLabel = _selected.LegalName,
            WhatArrived = ArrivedBox.Text.Trim(), ActionTaken = ActionBox.Text.Trim(),
            Date = DateTimeOffset.Now,
        });
        await _mailStore.SaveAsync(mail);
        ArrivedBox.Text = ActionBox.Text = string.Empty;
        EntityList_SelectionChanged(this, null!);
    }
}
