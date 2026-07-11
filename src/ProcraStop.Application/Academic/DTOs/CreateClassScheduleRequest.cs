namespace ProcraStop.Application.Academic.DTOs;

public sealed class CreateClassScheduleRequest
{
    public Guid CourseId { get; init; }
    public DayOfWeek DayOfWeek { get; init; }
    public TimeSpan StartTime { get; init; }
    public TimeSpan EndTime { get; init; }
    public string? Classroom { get; init; }
    public string? Instructor { get; init; }
    public int ReminderMinutesBefore { get; init; }
}
