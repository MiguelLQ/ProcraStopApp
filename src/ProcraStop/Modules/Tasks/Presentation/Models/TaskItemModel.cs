using ProcraStop.Modules.Tasks.Application.DTOs;
using ProcraStop.Modules.Tasks.Domain.Enums;

namespace ProcraStop.Modules.Tasks.Presentation.Models
{
    public sealed class TaskItemModel
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = string.Empty;
        public string? Description { get; init; }
        public DateTime? DueDate { get; init; }
        public int? EstimatedMinutes { get; init; }
        public TaskPriority Priority { get; init; }
        public TaskItemStatus Status { get; init; }
        public bool IsOverdue { get; init; }
        public string? CourseName { get; set; }
        public string? CourseColor { get; set; }
        public bool HasCourse => !string.IsNullOrWhiteSpace(CourseName);
        public string StatusText => Status switch
        {
            TaskItemStatus.Pending => "Pendiente",
            TaskItemStatus.InProgress => "En progreso",
            TaskItemStatus.Completed => "Completada",
            _ => "Pendiente"
        };

        public string PriorityText => Priority switch
        {
            TaskPriority.Low => "Baja",
            TaskPriority.Medium => "Media",
            TaskPriority.High => "Alta",
            _ => "Media"
        };

        /// <summary>
        /// Texto principal corto para mostrar la fecha en el item.
        /// Ej:
        /// Hoy · 18:00
        /// Mañana · 09:30
        /// Vie · 14:00
        /// 12/04 · 16:00
        /// Sin fecha límite
        /// </summary>
        public string DueDatePrimaryText
        {
            get
            {
                if (!DueDate.HasValue)
                    return "Sin fecha límite";

                var due = DueDate.Value;
                var today = DateTime.Today;
                var tomorrow = today.AddDays(1);

                if (due.Date == today)
                    return $"Hoy · {due:HH:mm}";

                if (due.Date == tomorrow)
                    return $"Mañana · {due:HH:mm}";

                if (due.Date <= today.AddDays(6))
                    return $"{GetShortDayName(due.DayOfWeek)} · {due:HH:mm}";

                return $"{due:dd/MM} · {due:HH:mm}";
            }
        }

        /// <summary>
        /// Texto relativo/estado del vencimiento.
        /// Ej:
        /// Vence en 2h
        /// Vence en 35 min
        /// Vence hoy
        /// Vence mañana
        /// Vencida hace 35 min
        /// Vencida ayer
        /// </summary>
        public string DueDateRelativeText
        {
            get
            {
                if (!DueDate.HasValue)
                    return string.Empty;

                var now = DateTime.Now;
                var due = DueDate.Value;
                var diff = due - now;

                // Si ya venció
                if (diff.TotalMinutes < 0)
                {
                    var overdue = now - due;

                    if (overdue.TotalMinutes < 60)
                        return $"Vencida hace {Math.Max(1, (int)overdue.TotalMinutes)} min";

                    if (overdue.TotalHours < 24 && due.Date == now.Date)
                        return $"Vencida hace {(int)overdue.TotalHours}h";

                    if (due.Date == now.Date.AddDays(-1))
                        return "Vencida ayer";

                    return $"Vencida hace {(int)overdue.TotalDays} día(s)";
                }

                // Si aún no vence
                if (diff.TotalMinutes < 60)
                    return $"Vence en {Math.Max(1, (int)diff.TotalMinutes)} min";

                if (diff.TotalHours < 24 && due.Date == now.Date)
                    return $"Vence en {(int)diff.TotalHours}h";

                if (due.Date == now.Date)
                    return "Vence hoy";

                if (due.Date == now.Date.AddDays(1))
                    return "Vence mañana";

                if (diff.TotalDays < 7)
                    return $"Vence en {(int)Math.Ceiling(diff.TotalDays)} día(s)";

                return string.Empty;
            }
        }

        /// <summary>
        /// Mantener temporalmente por compatibilidad si tu XAML actual usa DueDateText.
        /// Luego puedes reemplazarlo por DueDatePrimaryText en la UI.
        /// </summary>
        public string DueDateText => DueDatePrimaryText;

        public string EstimatedMinutesText =>
            EstimatedMinutes.HasValue
                ? $"Requerido: {EstimatedMinutes.Value} min"
                : "Sin estimación";

        public string OverdueText => IsActuallyOverdue ? "Vencida" : string.Empty;

        public bool CanComplete => Status != TaskItemStatus.Completed;
        public bool HasDescription => !string.IsNullOrWhiteSpace(Description);

        public bool HasDueDate => DueDate.HasValue;

        public bool HasDueDateRelativeText => !string.IsNullOrWhiteSpace(DueDateRelativeText);

        /// <summary>
        /// Mejor para UI que depender solo del DTO.
        /// </summary>
        public bool IsActuallyOverdue =>
            DueDate.HasValue &&
            Status != TaskItemStatus.Completed &&
            DueDate.Value < DateTime.Now;

        public bool ShowOverdue => IsActuallyOverdue;
        
        public static TaskItemModel FromDto(TaskListItemDto dto)
        {
            return new TaskItemModel
            {
                Id = dto.Id,
                Title = dto.Title,
                Description = dto.Description,
                DueDate = dto.DueDate,
                EstimatedMinutes = dto.EstimatedMinutes,
                Priority = dto.Priority,
                Status = dto.Status,
                IsOverdue = dto.IsOverdue,
                // mapear curso
                CourseName = dto.CourseName,
                CourseColor = dto.CourseColor ?? "#6B7280"
            };  
        }

        private static string GetShortDayName(DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Monday => "Lun",
                DayOfWeek.Tuesday => "Mar",
                DayOfWeek.Wednesday => "Mié",
                DayOfWeek.Thursday => "Jue",
                DayOfWeek.Friday => "Vie",
                DayOfWeek.Saturday => "Sáb",
                DayOfWeek.Sunday => "Dom",
                _ => "Día"
            };
        }
        public bool IsDueSoon =>
            DueDate.HasValue &&
            Status != TaskItemStatus.Completed &&
            DueDate.Value >= DateTime.Now &&
            DueDate.Value <= DateTime.Now.AddHours(24);

        public string DueDateRelativeColorHex
        {
            get
            {
                if (IsActuallyOverdue)
                    return "#DC2626"; // rojo

                if (IsDueSoon)
                    return "#F59E0B"; // ámbar/naranja

                return "#6B7280"; // gris
            }
        }

        public string PriorityColorHex => Priority switch
        {
            TaskPriority.High => "#DC2626",   // rojo
            TaskPriority.Medium => "#F59E0B", // ámbar
            TaskPriority.Low => "#2563EB",    // azul
            _ => "#6B7280"
        };

        public string StatusColorHex => Status switch
        {
            TaskItemStatus.Pending => "#6B7280",     // gris
            TaskItemStatus.InProgress => "#2563EB",  // azul
            TaskItemStatus.Completed => "#16A34A",   // verde
            _ => "#6B7280"
        };
    }
}