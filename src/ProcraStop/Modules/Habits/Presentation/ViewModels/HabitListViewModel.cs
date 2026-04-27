

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProcraStop.Modules.Habits.Application.Queries.GetAllHabits;
using ProcraStop.Modules.Habits.Presentation.Models;
using ProcraStop.Modules.Habits.Presentation.Views;
using System.Collections.ObjectModel;

namespace ProcraStop.Modules.Habits.Presentation.ViewModels
{
    public partial class HabitListViewModel : ObservableObject
    {
        private readonly GetAllHabitsHandler _handler;

        public HabitListViewModel(GetAllHabitsHandler handler)
        {
            _handler = handler;
        }
        [RelayCommand]
        private async Task OpenHabit(Guid habitId)
        {
            Console.WriteLine($"🔥 CLICK Habit: {habitId}");

            await Shell.Current.GoToAsync($"habit-track?habitId={habitId}");

        }
        public ObservableCollection<HabitItemViewModel> Habits { get; } = new();
        [RelayCommand]
        private async Task GoToCreate()
        {
            await Shell.Current.GoToAsync(nameof(CreateHabitPage));
        }

        [RelayCommand]
        public async Task Load()
        {
            Habits.Clear();

            var habits = await _handler.Handle(new GetAllHabitsQuery());

            foreach (var h in habits)
            {
                Habits.Add(new HabitItemViewModel
                {
                    Id = h.Id,
                    Name = h.Name,
                    Unit = h.Unit.ToString(),
                    Target = h.Target.Value,
                    IsActive = h.IsActive,
                    Weight = h.Weight
                });
            }
        }
      
    }
}
