
using ProcraStop.Domain.Tasks.Enums;

namespace ProcraStop.Domain.Tasks.Entities
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
        public AcademicActivityType ActivityType { get; private set; }
        public DateTime? PlannedStartDate { get; private set; }
        public DateTime? StartedAtUtc { get; private set; }
        public string? FirstStep { get; private set; }
        public DateTime? PlannedReviewDate { get; private set; }
        public DateTime? ReviewedAtUtc { get; private set; }
        // 

        //NUEVO: Subtasks
        // Permite saber si la tarea está completa, considerando el estado de las subtareas
        public bool IsCompleted => Status == TaskItemStatus.Completed;
        // Permite saber si la tarea está en progreso, considerando el estado de las subtareas
        private readonly List<Subtask> _subtasks = [];
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
        // create
        public void AddSubtask(Guid id, string title, bool isCompleted = false, DateTime? plannedDate = null)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id inválido");

            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("La subtarea necesita título");
            ValidateMilestoneDate(plannedDate);

            _subtasks.Add(new Subtask(id, title, isCompleted, plannedDate));

            UpdateStatusBasedOnSubtasks();
        }
        // edit - genera id en la capa de dominio
        public void AddSubtask(string title, bool isCompleted = false, DateTime? plannedDate = null)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("La subtarea necesita título");
            ValidateMilestoneDate(plannedDate);

            _subtasks.Add(new Subtask(Guid.NewGuid(), title, isCompleted, plannedDate));

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
            if (ActivityType == AcademicActivityType.Deliverable && !ReviewedAtUtc.HasValue)
                throw new InvalidOperationException("Revisa el entregable antes de marcarlo como entregado.");

            Start();
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

        public void Start()
        {
            StartedAtUtc ??= DateTime.UtcNow;
            if (Status == TaskItemStatus.Pending)
                Status = TaskItemStatus.InProgress;
        }

        public void MarkReviewed()
        {
            if (ActivityType != AcademicActivityType.Deliverable)
                throw new InvalidOperationException("Solo un entregable requiere revision.");
            if (_subtasks.Any(s => !s.IsCompleted))
                throw new InvalidOperationException("Termina los hitos antes de revisar el entregable.");

            Start();
            ReviewedAtUtc = DateTime.UtcNow;
        }

        public void SetAcademicPlan(
            AcademicActivityType activityType,
            DateTime? plannedStartDate,
            string? firstStep,
            DateTime? plannedReviewDate)
        {
            if (!Enum.IsDefined(activityType))
                throw new ArgumentOutOfRangeException(nameof(activityType));
            if (activityType == AcademicActivityType.Exam && !DueDate.HasValue)
                throw new InvalidOperationException("Un examen necesita fecha.");
            if (plannedStartDate.HasValue && DueDate.HasValue && plannedStartDate > DueDate)
                throw new InvalidOperationException("El inicio previsto no puede ser posterior al vencimiento.");
            if (plannedReviewDate.HasValue && activityType != AcademicActivityType.Deliverable)
                throw new InvalidOperationException("La revision prevista solo aplica a entregables.");
            if (plannedReviewDate.HasValue && DueDate.HasValue && plannedReviewDate > DueDate)
                throw new InvalidOperationException("La revision debe ser antes de la entrega.");
            if (plannedReviewDate.HasValue && plannedStartDate.HasValue && plannedReviewDate < plannedStartDate)
                throw new InvalidOperationException("La revision no puede ser anterior al inicio previsto.");
            foreach (var subtask in _subtasks)
                ValidateMilestoneDate(subtask.PlannedDate);

            ActivityType = activityType;
            PlannedStartDate = plannedStartDate;
            FirstStep = string.IsNullOrWhiteSpace(firstStep) ? null : firstStep.Trim();
            PlannedReviewDate = plannedReviewDate;
            if (activityType != AcademicActivityType.Deliverable)
                ReviewedAtUtc = null;
            UpdateStatusBasedOnSubtasks();
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
        public static StudyTask FromPersistence(
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
            List<Subtask> subtasks,
            AcademicActivityType activityType = AcademicActivityType.Task,
            DateTime? plannedStartDate = null,
            DateTime? startedAtUtc = null,
            string? firstStep = null,
            DateTime? plannedReviewDate = null,
            DateTime? reviewedAtUtc = null)
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
                CompletedAtUtc = completedAtUtc,
                ActivityType = activityType,
                PlannedStartDate = plannedStartDate,
                StartedAtUtc = startedAtUtc,
                FirstStep = firstStep,
                PlannedReviewDate = plannedReviewDate,
                ReviewedAtUtc = reviewedAtUtc
            };

            task.LoadSubtasks(subtasks);

            return task;
        }
        // Permite actualizar el estado de la tarea automáticamente al completar o reabrir subtareas    
        private void UpdateStatusBasedOnSubtasks()
        {
            if (!_subtasks.Any())
            {
                if (Status != TaskItemStatus.Completed)
                    Status = StartedAtUtc.HasValue ? TaskItemStatus.InProgress : TaskItemStatus.Pending;
                return;
            }

            var allCompleted = _subtasks.All(s => s.IsCompleted);
            var anyCompleted = _subtasks.Any(s => s.IsCompleted);

            if (allCompleted)
            {
                Start();
                if (ActivityType == AcademicActivityType.Deliverable)
                {
                    Status = TaskItemStatus.InProgress;
                    CompletedAtUtc = null;
                }
                else
                {
                    Status = TaskItemStatus.Completed;
                    CompletedAtUtc ??= DateTime.UtcNow;
                }
            }
            else if (anyCompleted)
            {
                Start();
                Status = TaskItemStatus.InProgress;
                CompletedAtUtc = null;
                ReviewedAtUtc = null;
            }
            else
            {
                Status = StartedAtUtc.HasValue ? TaskItemStatus.InProgress : TaskItemStatus.Pending;
                CompletedAtUtc = null;
                ReviewedAtUtc = null;
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
    List<Subtask> subtasks,
    AcademicActivityType activityType = AcademicActivityType.Task,
    DateTime? plannedStartDate = null,
    string? firstStep = null,
    DateTime? plannedReviewDate = null)
        {
            Title = title;
            Description = description;
            DueDate = dueDate;
            ReminderDate = reminderDate;
            Priority = priority;
            CourseId = courseId;
            EstimatedMinutes = estimatedMinutes;

            // =========================
            // SYNC REAL (SIN ROMPER)
            // =========================

            // 1. Remove
            var incomingIds = subtasks.Select(x => x.Id).ToHashSet();

            _subtasks.RemoveAll(s => !incomingIds.Contains(s.Id));

            // 2. Add / Update
            // 1. NUEVAS
            foreach (var dto in subtasks.Where(s => s.Id == Guid.Empty))
            {
                AddSubtask(dto.Title, dto.IsCompleted, dto.PlannedDate);
            }

            // 2. EXISTENTES
            foreach (var dto in subtasks.Where(s => s.Id != Guid.Empty))
            {
                var existing = _subtasks.FirstOrDefault(s => s.Id == dto.Id);

                if (existing != null)
                {
                    ValidateMilestoneDate(dto.PlannedDate);
                    existing.Update(dto.Title, dto.IsCompleted, dto.PlannedDate);
                }
                else
                {
                    // fallback por seguridad (datos corruptos o edge case)
                    AddSubtask(dto.Id, dto.Title, dto.IsCompleted, dto.PlannedDate);
                }
            }

            UpdateStatusBasedOnSubtasks();
            SetAcademicPlan(activityType, plannedStartDate, firstStep, plannedReviewDate);
        }
        // =========================
        // logica de dificultad
        // ========================
        public TaskEffort GetEffort()
        {
            if (!EstimatedMinutes.HasValue)
                return TaskEffort.Unknown;

            return EstimatedMinutes.Value switch
            {
                <= 30 => TaskEffort.Quick,
                <= 60 => TaskEffort.Medium,
                _ => TaskEffort.Heavy
            };
        }
        // =========================
        // HELPERS
        // =========================

        public bool IsOverdue =>
            DueDate.HasValue &&
            DueDate.Value < DateTime.Now &&
            Status != TaskItemStatus.Completed &&
            !IsArchived;

        private void ValidateMilestoneDate(DateTime? plannedDate)
        {
            if (plannedDate.HasValue && DueDate.HasValue && plannedDate.Value.Date > DueDate.Value.Date)
                throw new InvalidOperationException("La fecha del hito no puede ser posterior a la actividad.");
        }
    }
}
