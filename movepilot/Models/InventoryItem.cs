namespace MovePilot.Models;

public sealed class InventoryItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string MoveId { get; set; } = string.Empty;
    public string Room { get; set; } = "Living room";
    public string ItemName { get; set; } = string.Empty;
    public int Qty { get; set; } = 1;
    public decimal CubePerItem { get; set; }
    public decimal WeightPerItem { get; set; }
    public string Packing { get; set; } = "Owner-packed"; // Mover-packed / Owner-packed / Not packed
    public bool Special { get; set; }

    public decimal TotalCube => Qty * CubePerItem;
    public decimal TotalWeight => Qty * WeightPerItem;

    public string DisplayLine =>
        $"{Room}: {ItemName} x{Qty} - {TotalCube:0} cu ft, {TotalWeight:0} lb - {Packing}{(Special ? " - SPECIAL" : string.Empty)}";
}
