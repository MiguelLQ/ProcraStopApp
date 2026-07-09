using ProcraStop.Domain.Habits.Enums;

namespace ProcraStop.Application.Habits.DTOs;

// editbycodex: proyeccion lista para UI del dia, sin exponer entidades de dominio.
public sealed class HabitTodayItemDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public HabitUnit Unit { get; init; }
    public HabitTrackingType TrackingType { get; init; }
    public int TargetValue { get; init; }
    public int CurrentValue { get; init; }
    public double Progress { get; init; }
    public bool IsCompleted { get; init; }
    public int CurrentStreak { get; init; }
    public string ScheduleText { get; init; } = string.Empty;
}
