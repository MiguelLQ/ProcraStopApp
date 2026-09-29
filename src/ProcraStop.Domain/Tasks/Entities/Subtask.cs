

namespace ProcraStop.Domain.Tasks.Entities
{
    public sealed class Subtask
    {
        public Guid Id { get; private set; }
        public string Title { get; private set; } = string.Empty;
        public bool IsCompleted { get; private set; }
        public DateTime? PlannedDate { get; private set; }

        private Subtask() { } // EF
                              // Este constructor es para reconstruir desde la base de datos (EF)
        public Subtask(Guid id, string title, bool isCompleted, DateTime? plannedDate = null)
        {
            Id = id;
            Title = title;
            IsCompleted = isCompleted;
            PlannedDate = plannedDate;
        }

        public void MarkAsCompleted()
        {
            IsCompleted = true;
        }

        public void MarkAsPending()
        {
            IsCompleted = false;
        }

        public void UpdateTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("El título es requerido");

            Title = title;
        }
        // Este método permite actualizar tanto el título como el estado de completado en una sola operación,
        // lo que es útil para la edición rápida de subtareas.
        public void Update(string title, bool isCompleted, DateTime? plannedDate = null)
        {
            Title = title;
            PlannedDate = plannedDate;

            if (isCompleted)
                MarkAsCompleted();
            else
                MarkAsPending();
        }

    }
}
