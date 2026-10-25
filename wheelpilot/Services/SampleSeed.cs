using WheelPilot.Models;
using Windows.Storage;

namespace WheelPilot.Services;

public static class SampleSeed
{
    public static async Task SeedIfFirstRunAsync()
    {
        var folder = ApplicationData.Current.LocalFolder;
        if (await folder.TryGetItemAsync("wheel-seeded.flag") is not null) return;

        var studentStore = new JsonListStore<Student>("students.json");
        if ((await studentStore.LoadAsync()).Count > 0)
        {
            await folder.CreateFileAsync("wheel-seeded.flag", CreationCollisionOption.ReplaceExisting);
            return;
        }

        var i1 = new Instructor { Name = "Sample Instructor - Dana W.", Phone = "(555) 010-0601", LicenseNote = "Instructor licence on file (sample)" };
        var i2 = new Instructor { Name = "Sample Instructor - Marcus T.", Phone = "(555) 010-0602", LicenseNote = "Instructor licence on file (sample)" };
        await new JsonListStore<Instructor>("instructors.json").SaveAsync(new List<Instructor> { i1, i2 });

        var car1 = new Car
        {
            Name = "Car 1 - Sample Sedan", PlateText = "SAMPLE-1",
            InspectionExpiry = DateTimeOffset.Now.AddMonths(5),
            RegistrationExpiry = DateTimeOffset.Now.AddMonths(8),
            InsuranceExpiry = DateTimeOffset.Now.AddMonths(3),
        };
        var car2 = new Car
        {
            Name = "Car 2 - Sample Hatch", PlateText = "SAMPLE-2",
            InspectionExpiry = DateTimeOffset.Now.AddMonths(2),
            RegistrationExpiry = DateTimeOffset.Now.AddMonths(6),
            InsuranceExpiry = DateTimeOffset.Now.AddMonths(4),
            OutOfService = true,
            OutOfServiceNote = "In the shop tomorrow - brake pads (sample).",
        };
        await new JsonListStore<Car>("cars.json").SaveAsync(new List<Car> { car1, car2 });
        await new JsonListStore<MaintenanceEntry>("maintenance.json").SaveAsync(new List<MaintenanceEntry>
        {
            new() { CarId = car2.Id, CarLabel = car2.Name, Date = DateTimeOffset.Now.AddDays(1), Work = "Brake pads + inspection (sample, scheduled)", Cost = 240m },
        });

        var specs = new (string Name, string Status, string Course)[]
        {
            ("Sample Student - Avery J.", "In training", "Teen Course (sample)"),
            ("Sample Student - Ben K.", "In training", "Teen Course (sample)"),
            ("Sample Student - Chloe M.", "Test-ready", "Teen Course (sample)"),
            ("Sample Student - Diego R.", "Enrolled", "Teen Course (sample)"),
            ("Sample Student - Ella S.", "Inquiry", ""),
            ("Sample Student - Finn O.", "In training", "Adult Refresher (sample)"),
            ("Sample Student - Grace L.", "Passed", "Teen Course (sample)"),
            ("Sample Student - Henry P.", "Inquiry", ""),
        };
        var students = new List<Student>();
        foreach (var s in specs)
        {
            students.Add(new Student
            {
                Name = s.Name, Status = s.Status, Course = s.Course,
                ParentName = s.Status is "Inquiry" ? string.Empty : "Sample Parent",
                ParentPhone = "(555) 010-0610",
                PermitNumber = s.Status is "Inquiry" ? string.Empty : "PERMIT-SAMPLE",
                PermitExpiry = DateTimeOffset.Now.AddMonths(7),
                Source = "Referral",
                LastContacted = s.Name.Contains("Ella") ? DateTimeOffset.Now.AddDays(-9) : DateTimeOffset.Now.AddDays(-1),
                TestDate = s.Name.Contains("Chloe") ? DateTimeOffset.Now.AddDays(4) : null,
                TestNote = s.Name.Contains("Chloe") ? "Road test booked (sample)" : string.Empty,
            });
        }
        await studentStore.SaveAsync(students);

        var purchases = new List<Purchase>();
        var lessons = new List<Lesson>();
        var today = DateTimeOffset.Now.Date;
        DateTimeOffset Day(int offset) => new(today.AddDays(offset), TimeSpan.Zero);

        // Avery: bought 6h, used 4.5 -> 1.5 remaining.
        purchases.Add(new Purchase { StudentId = students[0].Id, StudentLabel = students[0].Name, PackageName = "6-hour Behind-the-Wheel", Hours = 6m, Price = 540m, PaidAmount = 540m, Date = DateTimeOffset.Now.AddDays(-20), Method = "Card (sample)" });
        for (var k = 0; k < 3; k++)
        {
            lessons.Add(new Lesson
            {
                StudentId = students[0].Id, StudentLabel = students[0].Name,
                InstructorId = i1.Id, InstructorLabel = i1.Name,
                CarId = car1.Id, CarLabel = car1.Name,
                Date = Day(-12 + k * 4), StartHour = 15m, DurationHours = 1.5m,
                Status = "Completed", SkillsCovered = "Residential driving (sample)",
            });
        }
        // Chloe: test-ready, bought 10h used 9, test in 4 days.
        purchases.Add(new Purchase { StudentId = students[2].Id, StudentLabel = students[2].Name, PackageName = "10-hour + Test Car", Hours = 10m, Price = 925m, PaidAmount = 925m, Date = DateTimeOffset.Now.AddDays(-40), Method = "Check (sample)" });
        lessons.Add(new Lesson
        {
            StudentId = students[2].Id, StudentLabel = students[2].Name,
            InstructorId = i2.Id, InstructorLabel = i2.Name, CarId = car1.Id, CarLabel = car1.Name,
            Date = Day(2), StartHour = 10m, DurationHours = 1m, Status = "Scheduled",
            SkillsCovered = "Mock road test (sample)", PickupLocation = "Home (sample)",
        });
        // Sample week: one instructor double-booking seeded deliberately - the board flags it.
        lessons.Add(new Lesson
        {
            StudentId = students[1].Id, StudentLabel = students[1].Name,
            InstructorId = i1.Id, InstructorLabel = i1.Name, CarId = car1.Id, CarLabel = car1.Name,
            Date = Day(1), StartHour = 15m, DurationHours = 1.5m, Status = "Scheduled", PickupLocation = "School (sample)",
        });
        lessons.Add(new Lesson
        {
            StudentId = students[5].Id, StudentLabel = students[5].Name,
            InstructorId = i1.Id, InstructorLabel = i1.Name, CarId = car1.Id, CarLabel = car1.Name,
            Date = Day(1), StartHour = 15.5m, DurationHours = 1.5m, Status = "Scheduled",
            Notes = "Seeded overlap - the schedule board flags this pair (sample).",
        });
        await new JsonListStore<Purchase>("purchases.json").SaveAsync(purchases);
        await new JsonListStore<Lesson>("lessons.json").SaveAsync(lessons);

        await new JsonListStore<MilestoneMark>("milestones.json").SaveAsync(new List<MilestoneMark>
        {
            new() { StudentId = students[2].Id, Skill = "Parking (parallel)", Date = DateTimeOffset.Now.AddDays(-6), SignedBy = i2.Name },
            new() { StudentId = students[2].Id, Skill = "Highway merging", Date = DateTimeOffset.Now.AddDays(-6), SignedBy = i2.Name },
            new() { StudentId = students[2].Id, Skill = "Mock road test", Date = DateTimeOffset.Now.AddDays(-2), SignedBy = i2.Name },
        });
        await folder.CreateFileAsync("wheel-seeded.flag", CreationCollisionOption.ReplaceExisting);
    }
}
