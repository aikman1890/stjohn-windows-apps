namespace CharterPilot.Models;

public sealed class Operator
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public string Contact { get; set; } = string.Empty;
    public string HomeBase { get; set; } = string.Empty;
    public string AircraftTypes { get; set; } = string.Empty;
    /// <summary>The broker's own record: reliability, catering standard, cancellation behavior, deadhead notes.</summary>
    public string Notes { get; set; } = string.Empty;
}

public sealed class Tail
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string OperatorId { get; set; } = string.Empty;
    public string OperatorLabel { get; set; } = string.Empty;
    public string TailNumber { get; set; } = string.Empty;
    public string AircraftType { get; set; } = string.Empty;
    public string HomeBase { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    public string DisplayLine => $"{TailNumber} - {AircraftType} - based {HomeBase} - {OperatorLabel}";
}
