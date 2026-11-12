using CommunityToolkit.Mvvm.ComponentModel;
using ProcraStop.Domain.Tasks.Enums;
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

        public AcademicActivityType ActivityType { get; set; }
        public Guid? CourseId { get; set; }
        public DateTime? PlannedStartDate { get; set; }
        public DateTime? StartedAtUtc { get; set; }
        public string? FirstStep { get; set; }
        public DateTime? PlannedReviewDate { get; set; }
        public DateTime? ReviewedAtUtc { get; set; }
        public bool IsDeliverable => ActivityType == AcademicActivityType.Deliverable;
        public bool IsExam => ActivityType == AcademicActivityType.Exam;
        public bool IsReading => ActivityType == AcademicActivityType.Reading;
        public string SubtasksSectionTitle => IsReading ? "Capítulos o secciones" : "Subtareas";
        public bool CanStart => StartedAtUtc is null && Status != TaskItemStatus.Completed;
        public bool CanReview => IsDeliverable && ReviewedAtUtc is null && Subtasks.All(x => x.IsCompleted);
        public string ActivityTypeText => ActivityType switch
        {
            AcademicActivityType.Reading => "Lectura",
            AcademicActivityType.Exam => "Examen",
            AcademicActivityType.Deliverable => "Entregable",
            _ => "Tarea"
        };
        public string AcademicPlanText =>
            $"Inicio previsto: {PlannedStartDate?.ToString("dd/MM/yyyy") ?? "sin fecha"} · " +
            $"Inicio real: {StartedAtUtc?.ToLocalTime().ToString("dd/MM/yyyy HH:mm") ?? "pendiente"}";
        public string ReviewText => ReviewedAtUtc.HasValue
            ? $"Revisado: {ReviewedAtUtc.Value.ToLocalTime():dd/MM/yyyy HH:mm}"
            : $"Revisión prevista: {PlannedReviewDate?.ToString("dd/MM/yyyy") ?? "sin fecha"}";

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
            TaskItemStatus.Completed when IsDeliverable => "Entregada",
            TaskItemStatus.InProgress when IsDeliverable && ReviewedAtUtc.HasValue => "Revisada, lista para entregar",
            TaskItemStatus.InProgress => "En progreso",
            TaskItemStatus.Completed => "Completada",
            _ => "Pendiente"
        };
        // ========================================= Progreso UI
        // El progreso se calcula como el porcentaje de subtareas completadas
        public double Progress =>
            Subtasks.Count == 0
                ? 0
                : (double)Subtasks.Count(s => s.IsCompleted) / Subtasks.Count;

        // El texto de progreso muestra "Sin subtareas" si no hay subtareas, o "X/Y completadas" si las hay
        public string ProgressText =>
            Subtasks.Count == 0
                ? "Sin subtareas"
                : IsReading
                    ? $"{Subtasks.Count(s => s.IsCompleted)}/{Subtasks.Count} capítulos completados"
                    : $"{Subtasks.Count(s => s.IsCompleted)}/{Subtasks.Count} completadas";
        public string ProgressColorHex =>
            Progress switch
            {
                >= 1 => "#16A34A", // verde completado
                >= 0.5 => "#F59E0B", // amarillo medio
                _ => "#2563EB" // azul inicio
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
        // 
        public bool CanComplete
        {
            get
            {
                if (Status == TaskItemStatus.Completed)
                    return false;

                if (IsDeliverable)
                    return ReviewedAtUtc.HasValue && Subtasks.All(x => x.IsCompleted);

                return Subtasks.Count == 0;
            }
        }
        // boton activado desactivado color
        public string CompleteButtonColor =>
            Status == TaskItemStatus.Completed
                ? "#9CA3AF"   // gris desactivado
                : "#2563EB";  // tu color primario
        // texto para boton 
        public string CompleteButtonText =>
        Status == TaskItemStatus.Completed
            ? (IsDeliverable ? "Entregada" : "Completada")
            : (IsDeliverable ? "Marcar entrega" : "Completar");
    }
}
