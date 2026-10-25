namespace RemodelPilot.Models;

public sealed class EstimateLine
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public decimal MarkupPercent { get; set; }
    public decimal Price => Math.Round(Cost * (1m + MarkupPercent / 100m), 2);

    public string DisplayLine => $"{Category}: {Description} - cost {Cost:C} + {MarkupPercent:0.#}% = {Price:C}";
}

public sealed class EstimateVersion
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string JobId { get; set; } = string.Empty;
    public string JobLabel { get; set; } = string.Empty;
    public int Version { get; set; } = 1;
    public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;
    public string Status { get; set; } = "Draft"; // Draft / Sent / Won / Lost
    public List<EstimateLine> Lines { get; set; } = new();
    public string Notes { get; set; } = string.Empty;

    public decimal TotalCost => Lines.Sum(l => l.Cost);
    public decimal TotalPrice => Lines.Sum(l => l.Price);

    public string DisplayLine => $"v{Version} - {JobLabel} - {TotalPrice:C} - {Status} - {Date:MMM d, yyyy}";
}
