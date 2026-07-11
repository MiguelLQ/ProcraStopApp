namespace ProcraStop.Domain.Academic.Entities;

public sealed class ClassSchedule
{
    public Guid Id { get; init; }
    public Guid CourseId { get; private set; }
    public DayOfWeek DayOfWeek { get; private set; }
    public TimeSpan StartTime { get; private set; }
    public TimeSpan EndTime { get; private set; }
    public string? Classroom { get; private set; }
    public string? Instructor { get; private set; }
    public int ReminderMinutesBefore { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; init; }

    private ClassSchedule()
    {
    }

    public static ClassSchedule Create(
        Guid courseId,
        DayOfWeek dayOfWeek,
        TimeSpan startTime,
        TimeSpan endTime,
        string? classroom,
        string? instructor,
        int reminderMinutesBefore)
    {
        Validate(courseId, startTime, endTime, reminderMinutesBefore);

        return new ClassSchedule
        {
            Id = Guid.NewGuid(),
            CourseId = courseId,
            DayOfWeek = dayOfWeek,
            StartTime = startTime,
            EndTime = endTime,
            Classroom = Normalize(classroom),
            Instructor = Normalize(instructor),
            ReminderMinutesBefore = reminderMinutesBefore,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public static ClassSchedule Restore(
        Guid id,
        Guid courseId,
        DayOfWeek dayOfWeek,
        TimeSpan startTime,
        TimeSpan endTime,
        string? classroom,
        string? instructor,
        int reminderMinutesBefore,
        bool isActive,
        DateTime createdAtUtc)
    {
        return new ClassSchedule
        {
            Id = id,
            CourseId = courseId,
            DayOfWeek = dayOfWeek,
            StartTime = startTime,
            EndTime = endTime,
            Classroom = classroom,
            Instructor = instructor,
            ReminderMinutesBefore = reminderMinutesBefore,
            IsActive = isActive,
            CreatedAtUtc = createdAtUtc
        };
    }

    public void Update(
        Guid courseId,
        DayOfWeek dayOfWeek,
        TimeSpan startTime,
        TimeSpan endTime,
        string? classroom,
        string? instructor,
        int reminderMinutesBefore)
    {
        Validate(courseId, startTime, endTime, reminderMinutesBefore);

        CourseId = courseId;
        DayOfWeek = dayOfWeek;
        StartTime = startTime;
        EndTime = endTime;
        Classroom = Normalize(classroom);
        Instructor = Normalize(instructor);
        ReminderMinutesBefore = reminderMinutesBefore;
    }

    public void Deactivate() => IsActive = false;

    private static void Validate(Guid courseId, TimeSpan startTime, TimeSpan endTime, int reminderMinutesBefore)
    {
        if (courseId == Guid.Empty)
            throw new ArgumentException("El curso es requerido.");

        if (endTime <= startTime)
            throw new ArgumentException("La hora de fin debe ser posterior a la hora de inicio.");

        if (reminderMinutesBefore < 0)
            throw new ArgumentException("El aviso no puede ser negativo.");
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
