using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProcraStop.Application.Habits.Commands.CreateHabit;
using ProcraStop.Domain.Habits.Enums;
using ProcraStop.Modules.Habits.Presentation.Models;
using ProcraStop.Shared.Services;
using System.Collections.ObjectModel;

namespace ProcraStop.Modules.Habits.Presentation.ViewModels;

public partial class CreateHabitViewModel : ObservableObject
{
    private readonly CreateHabitHandler _handler;

    [ObservableProperty] private string name = string.Empty;
    [ObservableProperty] private HabitUnitOption selectedUnit;
    [ObservableProperty] private HabitTrackingOption selectedTrackingType;
    [ObservableProperty] private int targetValue = 1;
    [ObservableProperty] private bool isBusy;

    public bool IsQuantitative => SelectedTrackingType.Value == HabitTrackingType.Quantitative;

    public ObservableCollection<HabitUnitOption> Units { get; } =
    [
        new("Minutos", HabitUnit.Minutes),
        new("Repeticiones", HabitUnit.Repetitions),
        new("Sesiones", HabitUnit.Sessions)
    ];

    public ObservableCollection<HabitTrackingOption> TrackingTypes { get; } =
    [
        new("Sí o no", "Márcalo con un solo toque", HabitTrackingType.Binary),
        new("Por cantidad", "Registra minutos, repeticiones o sesiones", HabitTrackingType.Quantitative)
    ];

    public ObservableCollection<DayItem> Days { get; }

    public CreateHabitViewModel(CreateHabitHandler handler)
    {
        _handler = handler;
        selectedUnit = Units[0];
        selectedTrackingType = TrackingTypes[0];
        Days = new ObservableCollection<DayItem>(
            Enum.GetValues<DayOfWeek>().Select(day =>
            {
                var item = new DayItem(day);
                item.IsSelected = true;
                return item;
            }));
    }

    partial void OnSelectedTrackingTypeChanged(HabitTrackingOption value)
    {
        OnPropertyChanged(nameof(IsQuantitative));
        if (value.Value == HabitTrackingType.Binary)
        {
            TargetValue = 1;
            SelectedUnit = Units.First(x => x.Value == HabitUnit.Sessions);
        }
    }

    [RelayCommand]
    private void ToggleDay(DayItem? day)
    {
        if (day is not null)
            day.IsSelected = !day.IsSelected;
    }

    [RelayCommand]
    private async Task Save()
    {
        if (IsBusy)
            return;

        if (string.IsNullOrWhiteSpace(Name))
        {
            PendingNotificationService.Set("Escribe un nombre para el hábito.", Shared.UI.Components.SnackbarType.Warning);
            return;
        }

        var selectedDays = Days.Where(day => day.IsSelected).Select(day => (int)day.Day).ToArray();
        if (selectedDays.Length == 0)
        {
            PendingNotificationService.Set("Selecciona al menos un día.", Shared.UI.Components.SnackbarType.Warning);
            return;
        }

        if (IsQuantitative && TargetValue <= 0)
        {
            PendingNotificationService.Set("El objetivo debe ser mayor que cero.", Shared.UI.Components.SnackbarType.Warning);
            return;
        }

        try
        {
            IsBusy = true;
            await _handler.Handle(new CreateHabitCommand
            {
                Name = Name.Trim(),
                Unit = IsQuantitative ? SelectedUnit.Value : HabitUnit.Sessions,
                TrackingType = SelectedTrackingType.Value,
                TargetValue = IsQuantitative ? TargetValue : 1,
                RequiredPercentage = 1.0,
                Weight = 1.0,
                DaysOfWeek = selectedDays
            });

            PendingNotificationService.Set("Hábito creado. Empieza con una acción pequeña.");
            await ShellNavigation.GoToAsync("..");
        }
        catch (Exception)
        {
            PendingNotificationService.Set("No se pudo crear el hábito.", Shared.UI.Components.SnackbarType.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
