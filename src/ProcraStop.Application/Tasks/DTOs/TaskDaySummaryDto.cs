namespace ProcraStop.Application.Tasks.DTOs
{
    // editbycodex: resumen ligero para pintar indicadores del calendario sin cargar tareas completas.
    public sealed class TaskDaySummaryDto
    {
        public DateTime Date { get; init; }
        public int TaskCount { get; init; }
        public int OverdueCount { get; init; }
    }
}
