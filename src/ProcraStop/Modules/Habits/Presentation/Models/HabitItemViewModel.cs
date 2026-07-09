using CommunityToolkit.Mvvm.ComponentModel;
using ProcraStop.Application.Habits.DTOs;
using ProcraStop.Domain.Habits.Enums;

namespace ProcraStop.Modules.Habits.Presentation.Models;

// editbycodex: estado observable del card para feedback inmediato.
public partial class HabitItemViewModel : ObservableObject
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public HabitUnit Unit { get; init; }
    public HabitTrackingType TrackingType { get; init; }
    public int Target { get; init; }
    public int CurrentStreak { get; init; }
    public string ScheduleText { get; init; } = string.Empty;
    public bool IsBinary => TrackingType == HabitTrackingType.Binary;
    public bool IsQuantitative => !IsBinary;

    [ObservableProperty] private int currentValue;
    [ObservableProperty] private double progress;
    [ObservableProperty] private bool isCompleted;
    [ObservableProperty] private bool isSaving;

    public string UnitText => Unit switch
    {
        HabitUnit.Minutes => "min",
        HabitUnit.Repetitions => "rep",
        HabitUnit.Sessions => "sesiones",
        _ => string.Empty
    };

    public string ProgressText => IsBinary
        ? (IsCompleted ? "Completado hoy" : "Pendiente")
        : $"{CurrentValue} de {Target} {UnitText}";

    public string StreakText => CurrentStreak == 1
        ? "1 día de constancia"
        : $"{CurrentStreak} días de constancia";

    partial void OnCurrentValueChanged(int value) => OnPropertyChanged(nameof(ProgressText));
    partial void OnIsCompletedChanged(bool value) => OnPropertyChanged(nameof(ProgressText));

    public static HabitItemViewModel FromDto(HabitTodayItemDto dto)
    {
        return new HabitItemViewModel
        {
            Id = dto.Id,
            Name = dto.Name,
            Unit = dto.Unit,
            TrackingType = dto.TrackingType,
            Target = dto.TargetValue,
            CurrentValue = dto.CurrentValue,
            Progress = dto.Progress,
            IsCompleted = dto.IsCompleted,
            CurrentStreak = dto.CurrentStreak,
            ScheduleText = dto.ScheduleText
        };
    }
}
