using ProcraStop.Modules.Tasks.Application.DTOs;
using ProcraStop.Modules.Tasks.Domain.Enums;


namespace ProcraStop.Modules.Tasks.Domain.Entities
{
    public sealed class StudyTask
    {
        public Guid Id { get; private set; }
        public string Title { get; private set; } = string.Empty;
        public string? Description { get; private set; }
        public DateTime? DueDate { get; private set; }
        public DateTime? ReminderDate { get; private set; }
        public TaskPriority Priority { get; private set; }
        public TaskItemStatus Status { get; private set; }
        public bool IsArchived { get; private set; }
        public Guid? CourseId { get; private set; }
        public int? EstimatedMinutes { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }
        public DateTime? CompletedAtUtc { get; private set; }
        // 

        //NUEVO: Subtasks
        // Permite saber si la tarea está completa, considerando el estado de las subtareas
        public bool IsCompleted => Status == TaskItemStatus.Completed;
        // Permite saber si la tarea está en progreso, considerando el estado de las subtareas
        private readonly List<Subtask> _subtasks = new();
        // Exponer las subtareas como una colección de solo lectura para mantener la encapsulación
        public IReadOnlyCollection<Subtask> Subtasks => _subtasks;

        private StudyTask() { } // EF

        // Factory method (mejor que constructor público)
        public static StudyTask Create(
            string title,
            string? description,
            DateTime? dueDate,
            TaskPriority priority,
            Guid? courseId)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("El título es requerido");
         
            return new StudyTask    
            {
                Id = Guid.NewGuid(),
                Title = title,
                Description = description,
                DueDate = dueDate,
                Priority = priority,
                CourseId = courseId,
                Status = TaskItemStatus.Pending,
                CreatedAtUtc = DateTime.UtcNow
            };
        }

        // =========================
        // SUBTASKS
        // =========================

        public void AddSubtask(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("La subtarea necesita título");

            _subtasks.Add(new Subtask(Guid.NewGuid(), title));

            UpdateStatusBasedOnSubtasks();
        }

        public void RemoveSubtask(Guid subtaskId)
        {
            var subtask = _subtasks.FirstOrDefault(s => s.Id == subtaskId);
            if (subtask is null) return;

            _subtasks.Remove(subtask);

            UpdateStatusBasedOnSubtasks();
        }


        public void ToggleSubtask(Guid subtaskId)
        {
            var subtask = _subtasks.FirstOrDefault(s => s.Id == subtaskId);
            if (subtask is null)
                throw new Exception("Subtarea no encontrada");

            if (subtask.IsCompleted)
                subtask.MarkAsPending();
            else
                subtask.MarkAsCompleted();

            UpdateStatusBasedOnSubtasks();
        }
        // =========================
        // REGLAS DE NEGOCIO
        // =========================

        public void Complete()
        {
            Status = TaskItemStatus.Completed;
            CompletedAtUtc = DateTime.UtcNow;

            foreach (var subtask in _subtasks)
                subtask.MarkAsCompleted();
        }

        public void Reopen()
        {
            Status = TaskItemStatus.InProgress;
            CompletedAtUtc = null;
        }

        // =========================
        // MÉTRICAS
        // =========================

        public double GetProgress()
        {
            if (!_subtasks.Any()) return 0;

            var completed = _subtasks.Count(s => s.IsCompleted);
            return (double)completed / _subtasks.Count;
        }
        // 
        public double Progress =>
            !_subtasks.Any()
                ? 0
                : (double)_subtasks.Count(s => s.IsCompleted) / _subtasks.Count;
        // Permite establecer o actualizar la fecha de recordatorio, con validaciones
        public void SetReminder(DateTime? reminderDate)
        {
            if (reminderDate.HasValue)
            {
                if (DueDate.HasValue && reminderDate > DueDate)
                    throw new InvalidOperationException("El recordatorio no puede ser posterior a la fecha límite.");

                if (reminderDate <= DateTime.Now)
                    throw new InvalidOperationException("El recordatorio no puede ser en el pasado.");
            }

            ReminderDate = reminderDate;
        }
        // Permite establecer o actualizar el tiempo estimado, con validaciones
        public void SetEstimatedMinutes(int? minutes)
        {
            if (minutes is < 0)
                throw new InvalidOperationException("El tiempo estimado no puede ser negativo.");

            EstimatedMinutes = minutes;
        }
        // Permite cargar las subtareas desde la persistencia, manteniendo la encapsulación
        internal void LoadSubtasks(List<Subtask> subtasks)
        {
            _subtasks.Clear();
            _subtasks.AddRange(subtasks);
        }
        // Permite reconstruir una tarea desde la persistencia, incluyendo sus subtareas
        internal static StudyTask FromPersistence(
            Guid id,
            string title,
            string? description,
            DateTime? dueDate,
            DateTime? reminderDate,
            TaskPriority priority,
            TaskItemStatus status,
            bool isArchived,
            Guid? courseId,
            int? estimatedMinutes,
            DateTime createdAtUtc,
            DateTime? completedAtUtc,
            List<Subtask> subtasks)
        {
            var task = new StudyTask
            {
                Id = id,
                Title = title,
                Description = description,
                DueDate = dueDate,
                ReminderDate = reminderDate,
                Priority = priority,
                Status = status,
                IsArchived = isArchived,
                CourseId = courseId,
                EstimatedMinutes = estimatedMinutes,
                CreatedAtUtc = createdAtUtc,
                CompletedAtUtc = completedAtUtc
            };

            task.LoadSubtasks(subtasks);

            return task;
        }
        // Permite actualizar el estado de la tarea automáticamente al completar o reabrir subtareas    
        private void UpdateStatusBasedOnSubtasks()
        {
            if (!_subtasks.Any())
            {
                Status = TaskItemStatus.Pending;
                CompletedAtUtc = null;
                return;
            }

            var allCompleted = _subtasks.All(s => s.IsCompleted);
            var anyCompleted = _subtasks.Any(s => s.IsCompleted);

            if (allCompleted)
            {
                Status = TaskItemStatus.Completed;
                CompletedAtUtc ??= DateTime.UtcNow;
            }
            else if (anyCompleted)
            {
                Status = TaskItemStatus.InProgress;
                CompletedAtUtc = null;
            }
            else
            {
                Status = TaskItemStatus.Pending;
                CompletedAtUtc = null;
            }
        }
        // archivar tarea
        public void Archive()
        {
            IsArchived = true;
        }
        // actualizar subatareas  
        public void Update(
      string title,
      string? description,
      DateTime? dueDate,
      DateTime? reminderDate,
      TaskPriority priority,
      Guid? courseId,
      int? estimatedMinutes,
      List<SubtaskDto> subtasks)
        {
            Title = title;
            Description = description;
            DueDate = dueDate;
            ReminderDate = reminderDate;
            Priority = priority;
            CourseId = courseId;
            EstimatedMinutes = estimatedMinutes;

            // 🔥 REEMPLAZO TOTAL (SOURCE OF TRUTH)
            _subtasks.Clear();

            foreach (var dto in subtasks)
            {
                if (dto.Id == Guid.Empty)
                    throw new Exception("Subtask debe tener Id");

                _subtasks.Add(new Subtask(dto.Id, dto.Title, dto.IsCompleted));
            }

            UpdateStatusBasedOnSubtasks();
        }
        // =========================
        // HELPERS
        // =========================

        public bool IsOverdue =>
            DueDate.HasValue &&
            DueDate.Value < DateTime.Now &&
            Status != TaskItemStatus.Completed &&
            !IsArchived;
    }
}
