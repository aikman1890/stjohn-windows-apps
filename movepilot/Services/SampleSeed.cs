using MovePilot.Models;
using Windows.Storage;

namespace MovePilot.Services;

public static class SampleSeed
{
    public static async Task SeedIfFirstRunAsync()
    {
        var folder = ApplicationData.Current.LocalFolder;
        if (await folder.TryGetItemAsync("move-seeded.flag") is not null) return;

        var moveStore = new JsonListStore<MoveJob>("moves.json");
        if ((await moveStore.LoadAsync()).Count == 0)
        {
            var move = new MoveJob
            {
                Customer = "Sample Customer (two-bedroom move)",
                Phone = "(555) 010-0300",
                Origin = "100 Sample St, Apt 4",
                Destination = "200 Sample Ave",
                MoveDate = DateTimeOffset.Now.AddDays(-6),
                Status = "Delivered",
                AccessNotes = "Origin: 2nd floor, no elevator. Destination: driveway fits one truck.",
                ValuationChoice = "Released value (sample - mover records the customer's own choice)",
                CrewSize = 3, Trucks = 1,
                ContractAmount = 1425m, Deposit = 200m, Collected = 1225m,
            };
            await moveStore.SaveAsync(new List<MoveJob> { move });

            await new JsonListStore<InventoryItem>("inventory.json").SaveAsync(new List<InventoryItem>
            {
                new() { MoveId = move.Id, Room = "Living room", ItemName = "Sofa (3-seat)", Qty = 1, CubePerItem = 60m, WeightPerItem = 150m, Packing = "Mover-packed" },
                new() { MoveId = move.Id, Room = "Living room", ItemName = "Coffee table", Qty = 1, CubePerItem = 10m, WeightPerItem = 40m, Packing = "Mover-packed" },
                new() { MoveId = move.Id, Room = "Bedroom", ItemName = "Queen bed set", Qty = 1, CubePerItem = 65m, WeightPerItem = 180m, Packing = "Mover-packed" },
                new() { MoveId = move.Id, Room = "Bedroom", ItemName = "Dresser", Qty = 1, CubePerItem = 30m, WeightPerItem = 120m, Packing = "Owner-packed" },
                new() { MoveId = move.Id, Room = "Kitchen", ItemName = "Box (medium)", Qty = 14, CubePerItem = 3m, WeightPerItem = 30m, Packing = "Owner-packed" },
                new() { MoveId = move.Id, Room = "Garage", ItemName = "Washer", Qty = 1, CubePerItem = 25m, WeightPerItem = 180m, Packing = "Not packed" },
            });

            await new JsonListStore<MoveEstimate>("estimates.json").SaveAsync(new List<MoveEstimate>
            {
                new()
                {
                    MoveId = move.Id, CustomerLabel = move.Customer, Version = 1, Type = "Hourly",
                    CrewSize = 3, Trucks = 1, HourlyRate = 150m, EstHours = 8m,
                    TravelFee = 75m, Materials = 150m, OtherFees = 0m,
                    Terms = "Deposit due at booking; balance on delivery before unloading.",
                    Status = "Approved", DecisionDate = DateTimeOffset.Now.AddDays(-9),
                    DecisionMethod = "Email reply", DecidedBy = "Sample Customer",
                    CreatedDate = DateTimeOffset.Now.AddDays(-10),
                },
            });

            await new JsonListStore<MoveClaim>("claims.json").SaveAsync(new List<MoveClaim>
            {
                new()
                {
                    MoveId = move.Id, CustomerLabel = move.Customer,
                    Item = "Coffee table - scratched leg (sample claim)",
                    AmountClaimed = 180m, FiledDate = DateTimeOffset.Now.AddDays(-4),
                    AckClockDays = 30, Status = "Open",
                    ProofLog = $"{DateTimeOffset.Now.AddDays(-4):yyyy-MM-dd} Claim received by phone; file opened (sample entry).",
                },
            });

            await new JsonListStore<StorageLot>("lots.json").SaveAsync(new List<StorageLot>
            {
                new()
                {
                    Customer = "Sample Storage Customer", LotId = "A-12",
                    InDate = DateTimeOffset.Now.AddDays(-45), MonthlyRate = 95m, PaidToDate = 95m,
                    Notes = "Sample lot - overflow from a staged home sale.",
                },
            });
        }
        await folder.CreateFileAsync("move-seeded.flag", CreationCollisionOption.ReplaceExisting);
    }
}
