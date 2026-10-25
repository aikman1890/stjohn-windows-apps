using BayPilot.Models;
using Windows.Storage;

namespace BayPilot.Services;

/// <summary>
/// Seeds clearly-labeled fictional sample data on first run only: one
/// customer, one vehicle, and a half-finished RO with one approved line,
/// one declined line, and a recommendation - so the board, the history,
/// and the declined-work register all show themselves working.
/// </summary>
public static class SampleSeed
{
    public static async Task EnsureAsync(
        JsonListStore<Customer> customers,
        JsonListStore<Vehicle> vehicles,
        JsonListStore<RepairOrder> orders)
    {
        try
        {
            var folder = ApplicationData.Current.LocalFolder;
            if (await folder.TryGetItemAsync("baypilot-seeded.flag") is not null) return;

            var existing = await orders.LoadAsync();
            if (existing.Count == 0)
            {
                var customer = new Customer
                {
                    Name = "Sample Customer (SAMPLE)",
                    Phone = "(555) 010-0100",
                    Notes = "Fictional sample customer - delete any time.",
                };
                var vehicle = new Vehicle
                {
                    CustomerId = customer.Id,
                    CustomerLabel = customer.Name,
                    Year = 2019,
                    Make = "Sample",
                    Model = "Sedan",
                    Plate = "SAMPLE",
                    Mileage = 86400,
                    ServiceIntervalMonths = 6,
                    ServiceIntervalMiles = 5000,
                };
                var ro = new RepairOrder
                {
                    RoNumber = "1001",
                    CustomerId = customer.Id,
                    CustomerLabel = customer.Name,
                    VehicleId = vehicle.Id,
                    VehicleLabel = vehicle.DisplayLine,
                    Status = "Waiting approval",
                    OpenedDate = DateTimeOffset.Now.AddHours(-3),
                    PromisedDate = DateTimeOffset.Now.AddDays(1),
                    Tech = "Sam (sample)",
                    MileageIn = 86400,
                    Complaint = "SAMPLE repair order - brake noise and an oil change.",
                    Recommendations = "Rear brakes at 3mm - recommend replacement soon.",
                    Lines = new List<RoLine>
                    {
                        new() { Kind = "Labor", Description = "Oil change service (sample)", Quantity = 0.5m, Rate = 95m, Decision = "Approved" },
                        new() { Kind = "Part", Description = "Rear brake pads (sample)", Quantity = 1m, PartCost = 42m, MarkupPercent = 50m, Rate = 63m, Decision = "Declined", DeclinedReason = "Customer wants to wait until payday", FollowUpDate = DateTimeOffset.Now.AddDays(14) },
                    },
                };
                await customers.SaveAsync(new List<Customer> { customer });
                await vehicles.SaveAsync(new List<Vehicle> { vehicle });
                await orders.SaveAsync(new List<RepairOrder> { ro });
            }
            await folder.CreateFileAsync("baypilot-seeded.flag", CreationCollisionOption.ReplaceExisting);
        }
        catch (Exception)
        {
            // Seeding must never crash the app.
        }
    }
}
