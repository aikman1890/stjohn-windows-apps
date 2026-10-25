using WheelPilot.Models;

namespace WheelPilot.Services;

/// <summary>
/// The triple-resource rule: a lesson needs student + instructor + car at the
/// same time. Overlaps and out-of-service cars are hard blocks, not warnings.
/// </summary>
public static class ConflictCheck
{
    public static List<string> FindConflicts(Lesson candidate, List<Lesson> all, List<Car> cars)
    {
        var problems = new List<string>();
        if (candidate.Status == "Cancelled") return problems;

        var car = cars.FirstOrDefault(c => c.Id == candidate.CarId);
        if (car is not null && car.OutOfService)
            problems.Add($"CAR OUT OF SERVICE: {car.Name} - {car.OutOfServiceNote}");

        foreach (var other in all)
        {
            if (other.Id == candidate.Id || other.Status == "Cancelled") continue;
            if (other.Date.Date != candidate.Date.Date) continue;
            var overlap = candidate.StartHour < other.EndHour && other.StartHour < candidate.EndHour;
            if (!overlap) continue;
            if (other.InstructorId == candidate.InstructorId && !string.IsNullOrWhiteSpace(candidate.InstructorId))
                problems.Add($"INSTRUCTOR DOUBLE-BOOKED: {candidate.InstructorLabel} already has {other.StudentLabel} {other.StartHour:0.0}-{other.EndHour:0.0}h.");
            if (other.CarId == candidate.CarId && !string.IsNullOrWhiteSpace(candidate.CarId))
                problems.Add($"CAR DOUBLE-BOOKED: {candidate.CarLabel} is already out with {other.StudentLabel} {other.StartHour:0.0}-{other.EndHour:0.0}h.");
            if (other.StudentId == candidate.StudentId)
                problems.Add($"STUDENT DOUBLE-BOOKED: {candidate.StudentLabel} already has a lesson {other.StartHour:0.0}-{other.EndHour:0.0}h.");
        }
        return problems;
    }
}
