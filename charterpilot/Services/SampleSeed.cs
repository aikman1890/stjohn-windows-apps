using CharterPilot.Models;
using Windows.Storage;

namespace CharterPilot.Services;

public static class SampleSeed
{
    public static async Task SeedIfFirstRunAsync()
    {
        var folder = ApplicationData.Current.LocalFolder;
        if (await folder.TryGetItemAsync("charter-seeded.flag") is not null) return;

        var clientStore = new JsonListStore<BrokerClient>("clients.json");
        if ((await clientStore.LoadAsync()).Count > 0)
        {
            await folder.CreateFileAsync("charter-seeded.flag", CreationCollisionOption.ReplaceExisting);
            return;
        }

        var cl1 = new BrokerClient
        {
            Name = "Sample Client - Whitfield (fictional)", Company = "Sample Family Office",
            Email = "client@sample.invalid", Phone = "(555) 010-0801",
            BookerName = "Sample Assistant - Dana", BookerContact = "assistant@sample.invalid",
            Preferences = "Heavy jet preferred; specific FBO at destination; catering: light, no shellfish (sample).",
        };
        var cl2 = new BrokerClient
        {
            Name = "Sample Client - Okafor (fictional)", Company = "Sample Ventures",
            Email = "client2@sample.invalid", Phone = "(555) 010-0802",
            BookerName = "Books direct", Preferences = "Midsize is fine; price-sensitive; early departures (sample).",
        };
        await clientStore.SaveAsync(new List<BrokerClient> { cl1, cl2 });

        var op1 = new Operator
        {
            Name = "Sample Air Operator One", Contact = "ops@sample.invalid", HomeBase = "KTEB (sample)",
            AircraftTypes = "Heavy, super-midsize (sample)",
            Notes = "Reliable; catering standard high; firm cancellation inside 72h (sample notes).",
        };
        var op2 = new Operator
        {
            Name = "Sample Air Operator Two", Contact = "ops2@sample.invalid", HomeBase = "KPBI (sample)",
            AircraftTypes = "Midsize, light (sample)",
            Notes = "Fast quotes; one late reposition last season - watch positioning (sample notes).",
        };
        await new JsonListStore<Operator>("operators.json").SaveAsync(new List<Operator> { op1, op2 });
        await new JsonListStore<Tail>("tails.json").SaveAsync(new List<Tail>
        {
            new() { OperatorId = op1.Id, OperatorLabel = op1.Name, TailNumber = "N-SAMPLE1", AircraftType = "Heavy jet (sample)", HomeBase = "KTEB (sample)", Notes = "Wi-Fi strong; crew praised twice (sample)." },
            new() { OperatorId = op2.Id, OperatorLabel = op2.Name, TailNumber = "N-SAMPLE2", AircraftType = "Midsize jet (sample)", HomeBase = "KPBI (sample)" },
        });

        var now = DateTimeOffset.Now;
        await new JsonListStore<Quote>("quotes.json").SaveAsync(new List<Quote>
        {
            // Overdue follow-up (red).
            new()
            {
                ClientId = cl1.Id, ClientLabel = cl1.Name, FromText = "KTEB", ToText = "KPBI",
                DepartDate = now.AddDays(5), AircraftClass = "Heavy jet",
                OperatorId = op1.Id, OperatorLabel = op1.Name, TailLabel = "N-SAMPLE1",
                Cost = 38500m, PriceQuoted = 44900m, Currency = "USD", Status = "Sent",
                SentAt = now.AddHours(-9), LastTouchedAt = now.AddHours(-9), FollowUpHours = 4m,
                Notes = "SAMPLE: follow-up overdue - this is the board's whole job.",
            },
            // Due within the hour band (orange).
            new()
            {
                ClientId = cl2.Id, ClientLabel = cl2.Name, FromText = "K DAL (sample)", ToText = "KASE (sample)",
                DepartDate = now.AddDays(9), AircraftClass = "Midsize jet",
                OperatorId = op2.Id, OperatorLabel = op2.Name, TailLabel = "N-SAMPLE2",
                Cost = 19750m, PriceQuoted = 22800m, Currency = "USD", Status = "Negotiating",
                SentAt = now.AddHours(-3), LastTouchedAt = now.AddHours(-3), FollowUpHours = 8m,
            },
            // A lost one for the register.
            new()
            {
                ClientId = cl2.Id, ClientLabel = cl2.Name, FromText = "KPBI", ToText = "KTEB",
                DepartDate = now.AddDays(-2), AircraftClass = "Light jet",
                OperatorId = op2.Id, OperatorLabel = op2.Name,
                Cost = 9800m, PriceQuoted = 11900m, Currency = "USD", Status = "Lost",
                SentAt = now.AddDays(-4), LastTouchedAt = now.AddDays(-3), FollowUpHours = 4m,
                LostReason = "Price", Notes = "SAMPLE lost quote - reason coded.",
            },
        });

        await new JsonListStore<TripFile>("trips.json").SaveAsync(new List<TripFile>
        {
            new()
            {
                ClientLabel = cl1.Name, RouteLabel = "KOPF -> KTEB (sample)",
                OperatorLabel = op1.Name, TailLabel = "N-SAMPLE1",
                DepartAt = now.AddHours(30),
                AgreementSigned = true, OperatorConfirmed = true, PassengerListReceived = false,
                CateringOrdered = false, GroundTransportBooked = true, PaymentReceived = true,
                FinalPrice = 47200m, FinalOperatorCost = 40100m, OtherCosts = 350m, Currency = "USD",
                Notes = "SAMPLE: departs in ~30 hours with two checklist items open - the dashboard flags it.",
            },
        });

        await folder.CreateFileAsync("charter-seeded.flag", CreationCollisionOption.ReplaceExisting);
    }
}
