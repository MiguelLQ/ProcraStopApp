namespace ProcraStop.Application.Tasks.Queries.SearchTasks
{
    public sealed class SearchTasksQuery
    {
        public string SearchText { get; init; } = string.Empty;
        public bool IncludeArchived { get; init; }
        public DateTime? DueDate { get; init; } // editbycodex: la busqueda respeta el dia seleccionado.
        public bool WithoutDueDate { get; init; } // editbycodex: diferencia fecha nula de ausencia de filtro.
        public int MaxResults { get; init; } = 30;
    }
}
