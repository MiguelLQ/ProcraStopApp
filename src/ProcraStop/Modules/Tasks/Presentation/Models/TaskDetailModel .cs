using CommunityToolkit.Mvvm.ComponentModel;
using ProcraStop.Modules.Tasks.Domain.Enums;
using System.Collections.ObjectModel;


namespace ProcraStop.Modules.Tasks.Presentation.Models
{
    public partial class TaskDetailModel : ObservableObject
    {
        public Guid Id { get; set; }

        [ObservableProperty]
        private string title = string.Empty;

        [ObservableProperty]
        private string? description;

        [ObservableProperty]
        private DateTime? dueDate;

        [ObservableProperty]
        private TaskPriority priority;

        [ObservableProperty]
        private DateTime? reminderDate;
        partial void OnPriorityChanged(TaskPriority value)
            => OnPropertyChanged(nameof(PriorityText));

        [ObservableProperty]
        private TaskItemStatus status;

        partial void OnStatusChanged(TaskItemStatus value)
            => OnPropertyChanged(nameof(StatusText));

        [ObservableProperty]
        private string? courseName;

        [ObservableProperty]
        private string? courseColor;

        public ObservableCollection<SubtaskModel> Subtasks { get; set; } = new();

        // UI
        public string PriorityText => Priority switch
        {
            TaskPriority.High => "Alta",
            TaskPriority.Medium => "Media",
            TaskPriority.Low => "Baja",
            _ => "Media"
        };

        public string StatusText => Status switch
        {
            TaskItemStatus.Pending => "Pendiente",
            TaskItemStatus.InProgress => "En progreso",
            TaskItemStatus.Completed => "Completada",
            _ => "Pendiente"
        };

        public string DueDateText =>
            DueDate?.ToString("dd MMM yyyy HH:mm") ?? "Sin fecha";
        public string ReminderText =>
    ReminderDate?.ToString("dd MMM yyyy HH:mm") ?? "Sin recordatorio";
        partial void OnReminderDateChanged(DateTime? value)
    => OnPropertyChanged(nameof(ReminderText));
        public bool HasCourse => !string.IsNullOrWhiteSpace(CourseName);
        public bool HasDescription => !string.IsNullOrWhiteSpace(Description);
    }
}
