using KennelPilot.Models;
using Windows.Storage;

namespace KennelPilot.Services;

public static class SampleSeed
{
    public static async Task SeedIfFirstRunAsync()
    {
        var folder = ApplicationData.Current.LocalFolder;
        if (await folder.TryGetItemAsync("kennel-seeded.flag") is not null) return;

        var ownerStore = new JsonListStore<Owner>("owners.json");
        var petStore = new JsonListStore<Pet>("pets.json");
        var stayStore = new JsonListStore<Stay>("stays.json");
        if ((await ownerStore.LoadAsync()).Count == 0)
        {
            var owner = new Owner
            {
                Name = "Sample Owner",
                Phone = "(555) 010-0100",
                VetName = "Sample Veterinary Clinic",
                VetPhone = "(555) 010-0199",
            };
            await ownerStore.SaveAsync(new List<Owner> { owner });
            var pet = new Pet
            {
                OwnerId = owner.Id,
                OwnerLabel = owner.Name,
                Name = "Sample Dog",
                Species = "Dog",
                Breed = "Mixed",
                WeightLb = 42m,
                Feeding = "Sample: 2 cups morning and evening (edit or delete).",
                Vaccinations = new List<Vaccination>
                {
                    new() { Name = "Rabies", Expiry = DateTimeOffset.Now.AddMonths(8) },
                    new() { Name = "DHPP", Expiry = DateTimeOffset.Now.AddMonths(5) },
                    new() { Name = "Bordetella", Expiry = DateTimeOffset.Now.AddMonths(2) },
                },
            };
            await petStore.SaveAsync(new List<Pet> { pet });
            await stayStore.SaveAsync(new List<Stay>
            {
                new()
                {
                    PetId = pet.Id,
                    PetLabel = pet.FullLabel,
                    OwnerId = owner.Id,
                    OwnerLabel = owner.Name,
                    RunLabel = "Run 1",
                    Status = "Checked in",
                    CheckIn = DateTimeOffset.Now.AddDays(-1),
                    ExpectedOut = DateTimeOffset.Now.AddDays(2),
                    VaccineSnapshot = "Sample stay - delete from the Board page.",
                },
            });
        }
        await folder.CreateFileAsync("kennel-seeded.flag", CreationCollisionOption.ReplaceExisting);
    }
}
