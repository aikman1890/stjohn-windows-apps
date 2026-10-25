namespace KennelPilot.Models;

public sealed class Vaccination
{
    public string Name { get; set; } = string.Empty;
    public DateTimeOffset? Expiry { get; set; }
    public string DocumentRef { get; set; } = string.Empty;
}

public sealed class Pet
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string OwnerId { get; set; } = string.Empty;
    public string OwnerLabel { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Species { get; set; } = "Dog";
    public string Breed { get; set; } = string.Empty;
    public decimal WeightLb { get; set; }
    public string AgeNote { get; set; } = string.Empty;
    public bool SpayedNeutered { get; set; }
    public string Feeding { get; set; } = string.Empty;
    public string Medications { get; set; } = string.Empty;
    public string Behavior { get; set; } = string.Empty;
    public List<Vaccination> Vaccinations { get; set; } = new();
    public string Notes { get; set; } = string.Empty;

    public string FullLabel => $"{Name} ({Species}{(string.IsNullOrWhiteSpace(Breed) ? "" : $" {Breed}")}) - {OwnerLabel}";

    /// <summary>Worst vaccination state for a stay starting on the given date.</summary>
    public (string State, string Detail) VaccineStateOn(DateTimeOffset start)
    {
        if (Vaccinations.Count == 0)
            return ("unknown", "no vaccination records on file");
        var expired = Vaccinations.Where(v => v.Expiry is { } e && e.Date < start.Date).ToList();
        if (expired.Count > 0)
            return ("expired", $"{expired[0].Name} expired {expired[0].Expiry:MMM d, yyyy}");
        var during = Vaccinations.Where(v => v.Expiry is { } e && e.Date >= start.Date).OrderBy(v => v.Expiry).FirstOrDefault();
        return ("valid", during is null ? "valid" : $"earliest expiry {during.Name} {during.Expiry:MMM d, yyyy}");
    }
}
