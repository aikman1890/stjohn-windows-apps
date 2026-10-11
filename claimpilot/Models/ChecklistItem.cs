using CommunityToolkit.Mvvm.ComponentModel;

namespace ClaimPilot.Models;

/// <summary>A checkable list item used by worksheets, checklists, and the incident template.</summary>
public partial class ChecklistItem : ObservableObject
{
    [ObservableProperty]
    private bool _isChecked;

    [ObservableProperty]
    private string _text = string.Empty;

    public ChecklistItem()
    {
    }

    public ChecklistItem(string text, bool isChecked = false)
    {
        _text = text;
        _isChecked = isChecked;
    }
}
