namespace ProcraStop.Application.Habits.Queries.GetTodayHabits;

public sealed class GetTodayHabitsQuery
{
    public DateTime Date { get; init; } = DateTime.Today;
}
