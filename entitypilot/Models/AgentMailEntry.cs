namespace EntityPilot.Models;

public sealed class AgentMailEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string EntityId { get; set; } = string.Empty;
    public string EntityLabel { get; set; } = string.Empty;
    public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;
    public string WhatArrived { get; set; } = string.Empty;
    public string ActionTaken { get; set; } = string.Empty;

    public string DisplayLine => $"{Date:MMM d, yyyy} - {EntityLabel} - {WhatArrived} -> {ActionTaken}";
}
