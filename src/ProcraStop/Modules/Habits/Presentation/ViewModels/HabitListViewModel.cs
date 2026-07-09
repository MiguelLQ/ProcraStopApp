using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProcraStop.Application.Habits.Commands.TrackHabit;
using ProcraStop.Application.Habits.Queries.GetTodayHabits;
using ProcraStop.Modules.Habits.Presentation.Models;
using ProcraStop.Modules.Habits.Presentation.Views;
using ProcraStop.Shared.Services;
using ProcraStop.Shared.UI.Components;
using System.Collections.ObjectModel;
using System.Globalization;

namespace ProcraStop.Modules.Habits.Presentation.ViewModels;

public partial class HabitListViewModel : ObservableObject
{
    private readonly GetTodayHabitsHandler _todayHandler;
    private readonly TrackHabitHandler _trackHandler;
    private readonly ISnackbarService _snackbar;

    [ObservableProperty] private ObservableCollection<HabitItemViewModel> habits = new();
    [ObservableProperty] private IReadOnlyList<HabitCalendarDayItem> weekDays = Array.Empty<HabitCalendarDayItem>();
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string summaryText = "Preparando tu día";
    [ObservableProperty] private double dailyProgress;
    [ObservableProperty] private string selectedDateText = string.Empty;
    [ObservableProperty] private bool canEditSelectedDate = true;

    private DateTime _selectedDate = DateTime.Today;

    public HabitListViewModel(
        GetTodayHabitsHandler todayHandler,
        TrackHabitHandler trackHandler,
        ISnackbarService snackbar)
    {
        _todayHandler = todayHandler;
        _trackHandler = trackHandler;
        _snackbar = snackbar;
        BuildWeek();
        UpdateSelectedDateText();
    }

    [RelayCommand]
    public async Task Load()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            var result = await _todayHandler.Handle(
                new GetTodayHabitsQuery { Date = _selectedDate },
                CancellationToken.None);
            Habits = new ObservableCollection<HabitItemViewModel>(
                result.Select(HabitItemViewModel.FromDto));
            UpdateSummary();
        }
        catch (Exception)
        {
            await _snackbar.ShowErrorAsync("No se pudieron cargar tus hábitos.", SnackbarType.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SelectDate(HabitCalendarDayItem? day)
    {
        if (day is null || day.Date.Date == _selectedDate.Date)
            return;

        _selectedDate = day.Date.Date;
        CanEditSelectedDate = _selectedDate <= DateTime.Today; // editbycodex: fechas futuras son consulta, no registro.
        BuildWeek();
        UpdateSelectedDateText();
        await Load();
    }

    [RelayCommand]
    private async Task ToggleHabit(HabitItemViewModel? habit)
    {
        if (habit is null || !habit.IsBinary || habit.IsSaving)
            return;

        await SaveValue(habit, habit.IsCompleted ? 0 : 1);
    }

    [RelayCommand]
    private async Task IncreaseHabit(HabitItemViewModel? habit)
    {
        if (habit is null || !habit.IsQuantitative || habit.IsSaving)
            return;

        var step = habit.Unit == Domain.Habits.Enums.HabitUnit.Minutes ? 5 : 1;
        await SaveValue(habit, Math.Min(habit.Target, habit.CurrentValue + step));
    }

    [RelayCommand]
    private async Task DecreaseHabit(HabitItemViewModel? habit)
    {
        if (habit is null || !habit.IsQuantitative || habit.IsSaving)
            return;

        var step = habit.Unit == Domain.Habits.Enums.HabitUnit.Minutes ? 5 : 1;
        await SaveValue(habit, Math.Max(0, habit.CurrentValue - step));
    }

    [RelayCommand]
    private async Task OpenHabit(Guid habitId)
    {
        await Shell.Current.GoToAsync($"habit-track?habitId={habitId}");
    }

    [RelayCommand]
    private async Task GoToCreate()
    {
        await Shell.Current.GoToAsync(nameof(CreateHabitPage));
    }

    private async Task SaveValue(HabitItemViewModel habit, int value)
    {
        if (!CanEditSelectedDate)
        {
            await _snackbar.ShowInfoAsync("Podrás registrarlo cuando llegue ese día.", SnackbarType.Info);
            return;
        }

        habit.IsSaving = true;
        try
        {
            await _trackHandler.Handle(new TrackHabitCommand
            {
                HabitId = habit.Id,
                Date = _selectedDate,
                Value = value
            });

            await Load(); // editbycodex: vuelve a calcular progreso, orden y racha desde la fuente real.
            await _snackbar.ShowAsync(
                value >= habit.Target || (habit.IsBinary && value > 0)
                    ? "Hábito completado. Un paso más de constancia."
                    : "Progreso actualizado.",
                SnackbarType.Success);
        }
        catch (Exception)
        {
            await _snackbar.ShowErrorAsync("No se pudo guardar el progreso.", SnackbarType.Error);
        }
        finally
        {
            habit.IsSaving = false;
        }
    }

    private void UpdateSummary()
    {
        var completed = Habits.Count(x => x.IsCompleted);
        DailyProgress = Habits.Count == 0 ? 0 : (double)completed / Habits.Count;
        SummaryText = Habits.Count == 0
            ? "No tienes hábitos programados"
            : $"{completed} de {Habits.Count} completados";
    }

    private void BuildWeek()
    {
        var mondayOffset = ((int)_selectedDate.DayOfWeek + 6) % 7;
        var start = _selectedDate.AddDays(-mondayOffset);
        WeekDays = Enumerable.Range(0, 7)
            .Select(offset =>
            {
                var date = start.AddDays(offset);
                return new HabitCalendarDayItem
                {
                    Date = date,
                    DayName = date.ToString("ddd", new CultureInfo("es-ES")).TrimEnd('.'),
                    DayNumber = date.Day.ToString(),
                    IsToday = date.Date == DateTime.Today,
                    IsSelected = date.Date == _selectedDate.Date
                };
            })
            .ToList();
    }

    private void UpdateSelectedDateText()
    {
        SelectedDateText = _selectedDate.Date == DateTime.Today
            ? "Hoy"
            : _selectedDate.ToString("dddd, d MMMM", new CultureInfo("es-ES"));
    }
}
