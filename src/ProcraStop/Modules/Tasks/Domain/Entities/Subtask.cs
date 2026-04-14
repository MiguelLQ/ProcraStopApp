
namespace ProcraStop.Modules.Tasks.Domain.Entities
{
    public sealed class Subtask
    {
        public Guid Id { get; private set; }
        public string Title { get; private set; } = string.Empty;
        public bool IsCompleted { get; private set; }

        private Subtask() { } // EF

        internal Subtask(Guid id, string title)
        {
            Id = id;
            Title = title;
            IsCompleted = false;
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
        // Este constructor es para reconstruir desde la base de datos (EF)
        internal Subtask(Guid id, string title, bool isCompleted)
        {
            Id = id;
            Title = title;
            IsCompleted = isCompleted;
        }
    }
}
