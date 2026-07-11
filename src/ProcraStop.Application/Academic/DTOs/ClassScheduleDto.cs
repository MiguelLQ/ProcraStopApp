namespace ProcraStop.Application.Academic.DTOs;

public sealed class ClassScheduleDto
{
    public Guid Id { get; init; }
    public Guid CourseId { get; init; }
    public string CourseName { get; init; } = string.Empty;
    public string? CourseColor { get; init; }
    public DayOfWeek DayOfWeek { get; init; }
    public string DayName { get; init; } = string.Empty;
    public TimeSpan StartTime { get; init; }
    public TimeSpan EndTime { get; init; }
    public string TimeRangeText { get; init; } = string.Empty;
    public string? Classroom { get; init; }
    public string? Instructor { get; init; }
    public int ReminderMinutesBefore { get; init; }
}
