using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProcraStop.Modules.Tasks.Application.DTOs;
using ProcraStop.Modules.Tasks.Application.Facade;
using ProcraStop.Modules.Tasks.Domain.Enums;

namespace ProcraStop.Modules.Tasks.Presentation.ViewModels
{
    public partial class CreateTaskPageViewModel : ObservableObject
    {
        private readonly ITasksFacade _tasksFacade;

        [ObservableProperty]
        private string title = string.Empty;

        [ObservableProperty]
        private string? description;

        [ObservableProperty]
        private bool hasDueDate = true;

        [ObservableProperty]
        private DateTime selectedDueDate = DateTime.Today.AddDays(1);

        [ObservableProperty]
        private TimeSpan selectedDueTime = new TimeSpan(18, 0, 0);

        [ObservableProperty]
        private string estimatedMinutesText = "30";

        [ObservableProperty]
        private TaskPriority selectedPriority = TaskPriority.Medium;

        [ObservableProperty]
        private bool isSaving;

        [ObservableProperty]
        private int selectedReminderOffsetIndex = 0;
        // Exponemos la lista de prioridades para enlazarla a la UI
        public IReadOnlyList<TaskPriority> Priorities { get; } =
            Enum.GetValues<TaskPriority>();

        // Exponemos la lista de offsets de recordatorio para enlazarla a la UI
        public IReadOnlyList<ReminderOffset> ReminderOffsets { get; } =
            Enum.GetValues<ReminderOffset>();
       
        public ReminderOffset SelectedReminderOffset =>
    SelectedReminderOffsetIndex >= 0 &&
    SelectedReminderOffsetIndex < ReminderOffsets.Count
        ? ReminderOffsets[SelectedReminderOffsetIndex]
        : ReminderOffset.None;
        // Método para obtener el texto descriptivo de un ReminderOffset
        public string GetReminderText(ReminderOffset offset) => offset switch
        {
            ReminderOffset.None => "Sin recordatorio",
            ReminderOffset.OneMinuteBefore => "1 minuto antes (prueba)",
            ReminderOffset.FiveMinutesBefore => "5 minutos antes",
            ReminderOffset.ThirtyMinutesBefore => "30 minutos antes",
            ReminderOffset.OneHourBefore => "1 hora antes",
            ReminderOffset.OneDayBefore => "1 día antes",
            _ => "Personalizado"
        };
        public string ReminderPreviewText
        {
            get
            {
                if (!ComputedDueDate.HasValue || SelectedReminderOffset == ReminderOffset.None)
                    return "Sin recordatorio";

                var reminder = ComputedDueDate.Value.AddMinutes(-(int)SelectedReminderOffset);

                if (reminder <= DateTime.Now)
                    return "⚠️ El recordatorio sería en el pasado";

                return $"Recordatorio: {reminder:dd/MM HH:mm}";
            }
        }
        partial void OnSelectedReminderOffsetIndexChanged(int value)
        {
            OnPropertyChanged(nameof(ReminderPreviewText));
        }
        public event Action? TaskCreated;

        public CreateTaskPageViewModel(ITasksFacade tasksFacade)
        {
            _tasksFacade = tasksFacade;
        }

        public DateTime? ComputedDueDate =>
            HasDueDate
                ? SelectedDueDate.Date + SelectedDueTime
                : null;

        public string DueDatePreviewText
        {
            get
            {
                if (!ComputedDueDate.HasValue)
                    return "Sin fecha límite";

                var due = ComputedDueDate.Value;
                var today = DateTime.Today;
                var tomorrow = today.AddDays(1);

                if (due.Date == today)
                    return $"Vence hoy a las {due:HH:mm}";

                if (due.Date == tomorrow)
                    return $"Vence mañana a las {due:HH:mm}";

                if (due.Date <= today.AddDays(7))
                    return $"Vence esta semana · {due:ddd HH:mm}";

                return $"Vence el {due:dd/MM/yyyy HH:mm}";
            }
        }

        public bool CanSave =>
            !IsSaving &&
            !string.IsNullOrWhiteSpace(Title) &&
            IsEstimatedMinutesValid();

        partial void OnTitleChanged(string value)
        {
            SaveCommand.NotifyCanExecuteChanged();
            OnPropertyChanged(nameof(CanSave));
        }

        partial void OnEstimatedMinutesTextChanged(string value)
        {
            SaveCommand.NotifyCanExecuteChanged();
            OnPropertyChanged(nameof(CanSave));
        }

        partial void OnIsSavingChanged(bool value)
        {
            SaveCommand.NotifyCanExecuteChanged();
            OnPropertyChanged(nameof(CanSave));
        }

        partial void OnHasDueDateChanged(bool value)
        {
            OnPropertyChanged(nameof(ComputedDueDate));
            OnPropertyChanged(nameof(DueDatePreviewText));
        }

        partial void OnSelectedDueDateChanged(DateTime value)
        {
            OnPropertyChanged(nameof(ComputedDueDate));
            OnPropertyChanged(nameof(DueDatePreviewText));
            OnPropertyChanged(nameof(ReminderPreviewText));

        }

        partial void OnSelectedDueTimeChanged(TimeSpan value)
        {
            OnPropertyChanged(nameof(ComputedDueDate));
            OnPropertyChanged(nameof(DueDatePreviewText));
            OnPropertyChanged(nameof(ReminderPreviewText));
        }

        private bool IsEstimatedMinutesValid()
        {
            if (string.IsNullOrWhiteSpace(EstimatedMinutesText))
                return true;

            return int.TryParse(EstimatedMinutesText, out var minutes) && minutes > 0;
        }

        [RelayCommand(CanExecute = nameof(CanSave))]
        private async Task SaveAsync()
        {
            if (IsSaving)
                return;

            int? estimatedMinutes = null;

            if (!string.IsNullOrWhiteSpace(EstimatedMinutesText))
            {
                if (!int.TryParse(EstimatedMinutesText, out var minutes) || minutes <= 0)
                    return;

                estimatedMinutes = minutes;
            }

            try
            {
                IsSaving = true;

                var request = new CreateTaskRequest
                {
                    Title = Title.Trim(),
                    Description = string.IsNullOrWhiteSpace(Description)
                        ? null
                        : Description.Trim(),
                    DueDate = ComputedDueDate,
                    ReminderOffset = SelectedReminderOffset,
                    ReminderDate = null, // se ignora en backend
                    EstimatedMinutes = estimatedMinutes,
                    Priority = SelectedPriority,
                    CourseId = null
                };

                await _tasksFacade.CreateTaskAsync(request);
                Console.WriteLine($"OFFSET FINAL: {SelectedReminderOffset}");
                Console.WriteLine($"INDEX: {SelectedReminderOffsetIndex}");

                TaskCreated?.Invoke();
            }
            finally
            {
                IsSaving = false;
            }
        }
    }
}