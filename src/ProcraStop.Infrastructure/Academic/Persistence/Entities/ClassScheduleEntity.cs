namespace ProcraStop.Infrastructure.Academic.Persistence.Entities;

public sealed class ClassScheduleEntity
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public int DayOfWeek { get; set; }
    public long StartTicks { get; set; }
    public long EndTicks { get; set; }
    public string? Classroom { get; set; }
    public string? Instructor { get; set; }
    public int ReminderMinutesBefore { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public CourseEntity? Course { get; set; }
}
