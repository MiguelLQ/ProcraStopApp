using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProcraStop.Application.Habits.Commands.DeactivateHabit;
using ProcraStop.Application.Habits.Commands.TrackHabit;
using ProcraStop.Application.Habits.Queries.GetHabitById;
using ProcraStop.Application.Habits.Queries.GetHabitLogByDate;
using ProcraStop.Domain.Habits.Enums;
using ProcraStop.Shared.Services;
using ProcraStop.Shared.UI;
using ProcraStop.Shared.UI.Components;

namespace ProcraStop.Modules.Habits.Presentation.ViewModels;

[QueryProperty(nameof(HabitIdRaw), "habitId")]
public partial class TrackHabitViewModel : ObservableObject
{
    private readonly GetHabitByIdHandler _habitHandler;
    private readonly GetHabitLogByDateHandler _logHandler;
    private readonly TrackHabitHandler _trackHandler;
    private readonly DeactivateHabitHandler _deactivateHandler;
    private Guid _currentHabitId;

    [ObservableProperty] private string? habitIdRaw;
    [ObservableProperty] private string name = string.Empty;
    [ObservableProperty] private string unit = string.Empty;
    [ObservableProperty] private int targetValue;
    [ObservableProperty] private HabitTrackingType trackingType;
    [ObservableProperty] private bool isBinary;
    [ObservableProperty] private int value;
    [ObservableProperty] private bool isCompleted;
    [ObservableProperty] private bool alreadyLoggedToday;
    [ObservableProperty] private double progress;
    [ObservableProperty] private string progressText = string.Empty;
    [ObservableProperty] private bool isBusy;

    public bool IsQuantitative => !IsBinary;

    public TrackHabitViewModel(
        GetHabitByIdHandler habitHandler,
        GetHabitLogByDateHandler logHandler,
        TrackHabitHandler trackHandler,
        DeactivateHabitHandler deactivateHandler)
    {
        _habitHandler = habitHandler;
        _logHandler = logHandler;
        _trackHandler = trackHandler;
        _deactivateHandler = deactivateHandler;
    }

    partial void OnHabitIdRawChanged(string? value)
    {
        if (Guid.TryParse(value, out var id))
            _ = SafeUi.RunAsync(() => Load(id), "TrackHabitViewModel.Load");
    }

    partial void OnValueChanged(int value) => UpdateProgress();
    partial void OnIsCompletedChanged(bool value) => UpdateProgress();

    public async Task Load(Guid habitId)
    {
        _currentHabitId = habitId;
        var habit = await _habitHandler.Handle(new GetHabitByIdQuery { HabitId = habitId });
        if (habit is null)
            return;

        Name = habit.Name;
        Unit = habit.Unit switch
        {
            HabitUnit.Minutes => "minutos",
            HabitUnit.Repetitions => "repeticiones",
            HabitUnit.Sessions => "sesiones",
            _ => string.Empty
        };
        TargetValue = habit.Target.Value;
        TrackingType = habit.TrackingType;
        IsBinary = TrackingType == HabitTrackingType.Binary;
        OnPropertyChanged(nameof(IsQuantitative));

        var log = await _logHandler.Handle(new GetHabitLogByDateQuery
        {
            HabitId = habitId,
            Date = DateTime.Today
        });
        AlreadyLoggedToday = log is not null;
        Value = log?.Value ?? 0;
        IsCompleted = IsBinary && Value > 0;
        UpdateProgress();
    }

    [RelayCommand]
    private async Task Save()
    {
        if (IsBusy || _currentHabitId == Guid.Empty)
            return;

        try
        {
            IsBusy = true;
            var finalValue = IsBinary ? (IsCompleted ? 1 : 0) : Math.Max(0, Value);
            await _trackHandler.Handle(new TrackHabitCommand
            {
                HabitId = _currentHabitId,
                Date = DateTime.Today,
                Value = finalValue
            });
            PendingNotificationService.Set("Progreso del hábito actualizado.");
            await ShellNavigation.GoToAsync("..");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task Pause()
    {
        if (Shell.Current is not { } shell)
            return;

        var confirmed = await shell.DisplayAlertAsync(
            "Pausar hábito",
            "Dejará de aparecer en tu rutina, pero conservará su historial.",
            "Pausar",
            "Cancelar");
        if (!confirmed)
            return;

        await _deactivateHandler.Handle(
            new DeactivateHabitCommand { HabitId = _currentHabitId },
            CancellationToken.None);
        PendingNotificationService.Set("Hábito pausado.", SnackbarType.Info);
        await ShellNavigation.GoToAsync("..");
    }

    private void UpdateProgress()
    {
        var current = IsBinary ? (IsCompleted ? 1 : 0) : Value;
        Progress = TargetValue <= 0 ? 0 : Math.Min(1, (double)current / TargetValue);
        ProgressText = IsBinary
            ? (IsCompleted ? "Completado hoy" : "Pendiente para hoy")
            : $"{current} de {TargetValue} {Unit}";
    }
}
