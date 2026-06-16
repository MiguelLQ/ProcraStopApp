
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProcraStop.Application.Habits.Commands.CreateHabit;
using ProcraStop.Domain.Habits.Enums;
using ProcraStop.Modules.Habits.Presentation.Models;
using System.Collections.ObjectModel;

namespace ProcraStop.Modules.Habits.Presentation.ViewModels
{
    public partial class CreateHabitViewModel : ObservableObject
    {
        private readonly CreateHabitHandler _handler;

        public CreateHabitViewModel(CreateHabitHandler handler)
        {
            _handler = handler;

            Units = new ObservableCollection<HabitUnit>(
                Enum.GetValues(typeof(HabitUnit)).Cast<HabitUnit>());

            TrackingTypes = new ObservableCollection<HabitTrackingType>(
                Enum.GetValues(typeof(HabitTrackingType)).Cast<HabitTrackingType>());

            Days = new ObservableCollection<DayItem>(
                Enum.GetValues(typeof(DayOfWeek))
                    .Cast<DayOfWeek>()
                    .Select(d => new DayItem(d))
            );
        }

        // ----------- FIELDS -----------

        [ObservableProperty]
        private string name = string.Empty;

        [ObservableProperty]
        private HabitUnit selectedUnit;

        [ObservableProperty]
        private HabitTrackingType selectedTrackingType;

        [ObservableProperty]
        private int targetValue;

        [ObservableProperty]
        private double requiredPercentage = 1.0;

        [ObservableProperty]
        private double weight = 1.0;

        // ----------- COLLECTIONS -----------

        public ObservableCollection<HabitUnit> Units { get; }

        public ObservableCollection<HabitTrackingType> TrackingTypes { get; }

        public ObservableCollection<DayItem> Days { get; }

        // ----------- COMMAND -----------

        [RelayCommand]
        private async Task Save()
        {
            var selectedDays = Days
                .Where(d => d.IsSelected)
                .Select(d => (int)d.Day)
                .ToList();

            var command = new CreateHabitCommand
            {
                Name = Name,
                Unit = SelectedUnit,
                TrackingType = SelectedTrackingType,
                TargetValue = TargetValue,
                RequiredPercentage = RequiredPercentage,
                Weight = Weight,
                DaysOfWeek = selectedDays.ToArray()
            };

            await _handler.Handle(command);

            // reset básico
            Name = string.Empty;
            TargetValue = 0;
            RequiredPercentage = 1.0;
            Weight = 1.0;

            foreach (var d in Days)
                d.IsSelected = false;
            //  navegación hacia atrás
            await Shell.Current.GoToAsync("..");
        }
    }
}
