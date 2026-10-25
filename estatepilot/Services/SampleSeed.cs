using EstatePilot.Models;
using Windows.Storage;

namespace EstatePilot.Services;

public static class SampleSeed
{
    public static async Task SeedIfFirstRunAsync()
    {
        var folder = ApplicationData.Current.LocalFolder;
        if (await folder.TryGetItemAsync("estate-seeded.flag") is not null) return;

        var propertyStore = new JsonListStore<Property>("properties.json");
        if ((await propertyStore.LoadAsync()).Count > 0)
        {
            await folder.CreateFileAsync("estate-seeded.flag", CreationCollisionOption.ReplaceExisting);
            return;
        }

        // FICTIONAL sample residence - never a real family's details.
        var p1 = new Property
        {
            Name = "The Lake House (SAMPLE)", Address = "1 Sample Shore Rd (fictional)", Type = "Seasonal",
            SystemsNote = "HVAC (2 zones), standby generator, pool + heater, septic, security system serviced by Sample Alarm Co (sample).",
            AccessPointer = "Keys held by the house manager; alarm serviced by Sample Alarm Co - codes are NOT recorded in this app, by design (sample).",
            Notes = "Sample property seeded so every dashboard element is exercised.",
        };
        await propertyStore.SaveAsync(new List<Property> { p1 });

        await new JsonListStore<ServiceClock>("clocks.json").SaveAsync(new List<ServiceClock>
        {
            new() { PropertyId = p1.Id, PropertyLabel = p1.Name, Title = "Generator service (sample)", LastDoneDate = DateTimeOffset.Now.AddMonths(-13), NextDueDate = DateTimeOffset.Now.AddDays(-10), IntervalMonths = 12, Note = "SAMPLE: overdue - the storm-season failure this app exists to prevent." },
            new() { PropertyId = p1.Id, PropertyLabel = p1.Name, Title = "HVAC service (sample)", LastDoneDate = DateTimeOffset.Now.AddMonths(-6), NextDueDate = DateTimeOffset.Now.AddDays(24), IntervalMonths = 6 },
            new() { PropertyId = p1.Id, PropertyLabel = p1.Name, Title = "Pool opening (sample)", LastDoneDate = DateTimeOffset.Now.AddMonths(-6), NextDueDate = DateTimeOffset.Now.AddDays(70), IntervalMonths = 12 },
        });

        await new JsonListStore<Vendor>("vendors.json").SaveAsync(new List<Vendor>
        {
            new() { Name = "Sample Generator Services", Trade = "Generator", Contact = "(555) 010-0901 (sample)", PropertiesNote = "The Lake House (sample)", LastJobNote = "Annual service last season (sample)", RateNote = "Call-out $185 (sample)", CoiExpiry = DateTimeOffset.Now.AddDays(16), Notes = "SAMPLE: COI inside its orange window." },
            new() { Name = "Sample Pool Care", Trade = "Pool", Contact = "(555) 010-0902 (sample)", PropertiesNote = "The Lake House (sample)", CoiExpiry = DateTimeOffset.Now.AddMonths(8) },
        });

        await new JsonListStore<StaffMember>("staff.json").SaveAsync(new List<StaffMember>
        {
            new() { PropertyId = p1.Id, PropertyLabel = p1.Name, Name = "Sample Housekeeper - Ana R.", Role = "Housekeeper", SchedulePattern = "Tue / Thu / Sat (sample)", Contact = "(555) 010-0903 (sample)", StartDate = DateTimeOffset.Now.AddYears(-2) },
            new() { PropertyId = p1.Id, PropertyLabel = p1.Name, Name = "Sample Caretaker - Ben T.", Role = "Caretaker", SchedulePattern = "On call, weekly check (sample)", Contact = "(555) 010-0904 (sample)", StartDate = DateTimeOffset.Now.AddYears(-1), DocNote = "Background-check renewal (sample record)", DocExpiry = DateTimeOffset.Now.AddDays(45) },
        });

        await new JsonListStore<ManualSection>("manual.json").SaveAsync(new List<ManualSection>
        {
            new() { PropertyId = p1.Id, PropertyLabel = p1.Name, Section = "Standing instruction", Title = "Arrival setup (sample)", Body = "House at 72F by 3 PM on arrival days; coffee station stocked; lake-facing blinds open. (Sample manual entry.)" },
            new() { PropertyId = p1.Id, PropertyLabel = p1.Name, Section = "Principal preference", Title = "Pantry standards (sample)", Body = "Sparkling water, oat milk, and the standing grocery list in the kitchen binder. (Sample manual entry.)" },
            new() { PropertyId = p1.Id, PropertyLabel = p1.Name, Section = "Seasonal procedure", Title = "Winter close (sample)", Body = "Follow the 'Close the house' checklist; photograph every room on the final walk-through. (Sample manual entry.)" },
        });

        await new JsonListStore<JobEntry>("jobs.json").SaveAsync(new List<JobEntry>
        {
            new() { PropertyId = p1.Id, PropertyLabel = p1.Name, VendorLabel = "Sample Roofing Co", Date = DateTimeOffset.Now.AddMonths(-14), Work = "Roof replacement, main house (sample)", Cost = 28400m, Currency = "USD", InvoiceRef = "SAMPLE-INV-001", WarrantyExpiry = DateTimeOffset.Now.AddYears(9) },
        });

        await folder.CreateFileAsync("estate-seeded.flag", CreationCollisionOption.ReplaceExisting);
    }
}
