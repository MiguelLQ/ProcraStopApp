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
        private DateTime? reminderDate;

        [ObservableProperty]
        private TaskPriority priority;

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

        // UI PROPERTIES
        public string PriorityText => Priority switch
        {
            TaskPriority.High => "Prioridad: Alta",
            TaskPriority.Medium => "Prioridad: Media",
            TaskPriority.Low => "Prioridad: Baja",
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
        public bool IsActuallyOverdue =>
    DueDate.HasValue &&
    Status != TaskItemStatus.Completed &&
    DueDate.Value < DateTime.Now;

        public bool IsDueSoon =>
            DueDate.HasValue &&
            Status != TaskItemStatus.Completed &&
            DueDate.Value >= DateTime.Now &&
            DueDate.Value <= DateTime.Now.AddHours(24);
        public string PriorityColorHex => Priority switch
        {
            TaskPriority.High => "#DC2626",   // rojo
            TaskPriority.Medium => "#F59E0B", // ámbar
            TaskPriority.Low => "#2563EB",    // azul
            _ => "#6B7280"
        };

        public string StatusColorHex => Status switch
        {
            TaskItemStatus.Pending => "#6B7280",
            TaskItemStatus.InProgress => "#2563EB",
            TaskItemStatus.Completed => "#16A34A",
            _ => "#6B7280"
        };

        public string DueDateColorHex
        {
            get
            {
                if (IsActuallyOverdue)
                    return "#DC2626"; // rojo

                if (IsDueSoon)
                    return "#F59E0B"; // ámbar

                return "#6B7280"; // gris
            }
        }
    }
}
