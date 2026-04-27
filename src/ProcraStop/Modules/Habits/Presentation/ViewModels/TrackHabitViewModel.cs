

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProcraStop.Modules.Habits.Application.Commands.TrackHabit;
using ProcraStop.Modules.Habits.Application.Queries.GetHabitById;
using ProcraStop.Modules.Habits.Application.Queries.GetHabitLogByDate;
using ProcraStop.Modules.Habits.Domain.Enums;


namespace ProcraStop.Modules.Habits.Presentation.ViewModels
{
    [QueryProperty(nameof(HabitIdRaw), "habitId")]
    public partial class TrackHabitViewModel : ObservableObject
    {
        private readonly GetHabitByIdHandler _habitHandler;
        private readonly GetHabitLogByDateHandler _logHandler;
        private readonly TrackHabitHandler _trackHandler;

        public TrackHabitViewModel(
            GetHabitByIdHandler habitHandler,
            GetHabitLogByDateHandler logHandler,
            TrackHabitHandler trackHandler)
        {
            _habitHandler = habitHandler;
            _logHandler = logHandler;
            _trackHandler = trackHandler;
        }

        // ----------- NAV PARAM -----------

        [ObservableProperty]
        private string? habitIdRaw;

        // ----------- STATE -----------

        [ObservableProperty]
        private string name = string.Empty;

        [ObservableProperty]
        private string unit = string.Empty;

        [ObservableProperty]
        private HabitTrackingType trackingType;

        [ObservableProperty]
        private bool isBinary;

        // ----------- INPUT -----------

        [ObservableProperty]
        private int value;

        [ObservableProperty]
        private bool isCompleted;

        [ObservableProperty]
        private bool alreadyLoggedToday;

        // ----------- INIT -----------
        private Guid _currentHabitId;

        partial void OnHabitIdRawChanged(string? value)
        {
            Console.WriteLine($"🔥 OnHabitIdRawChanged TRIGGERED");
            Console.WriteLine($"👉 Raw value: {value}");

            if (string.IsNullOrWhiteSpace(value))
            {
                Console.WriteLine("❌ value vacío");
                return;
            }

            if (Guid.TryParse(value, out var guid))
            {
                Console.WriteLine($"✅ GUID válido: {guid}");
                _ = Load(guid);
            }
            else
            {
                Console.WriteLine("❌ GUID INVÁLIDO: " + value);
            }
        }
        public async Task Load(Guid habitId)
        {
            Console.WriteLine($"🚀 Load iniciado con ID: {habitId}");

            _currentHabitId = habitId;

            var today = DateTime.Today;

            var habit = await _habitHandler.Handle(
                new GetHabitByIdQuery { HabitId = habitId });

            if (habit == null)
            {
                Console.WriteLine("❌ Habit es NULL");
                return;
            }
            Console.WriteLine($"✅ Habit encontrado: {habit.Name}");
            Name = habit.Name;
            Unit = habit.Unit.ToString();
            TrackingType = habit.TrackingType;
            IsBinary = TrackingType == HabitTrackingType.Binary;

            var log = await _logHandler.Handle(
                new GetHabitLogByDateQuery
                {
                    HabitId = habitId,
                    Date = today
                });

            if (log != null)
            {
                Console.WriteLine("✅ Log encontrado para hoy");

                AlreadyLoggedToday = true;

                Value = log.Value;
                IsCompleted = log.Value > 0;
            }
            else
            {
                Console.WriteLine("⚠️ No hay log para hoy");

            }
        }
        // ----------- SAVE -----------

        [RelayCommand]
        private async Task Save()
        {
            int finalValue;

            if (IsBinary)
                finalValue = IsCompleted ? 1 : 0;
            else
                finalValue = Value;

            var command = new TrackHabitCommand
            {
                HabitId = _currentHabitId,
                Date = DateTime.Today,
                Value = finalValue,
                Notes = null
            };

            await _trackHandler.Handle(command);

            await Shell.Current.GoToAsync("..");
        }
        public void SetTaskId(Guid id)
        {
            _ = Load(id);
        }
    }
}
