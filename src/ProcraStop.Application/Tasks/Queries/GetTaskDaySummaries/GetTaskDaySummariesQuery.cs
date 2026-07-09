namespace ProcraStop.Application.Tasks.Queries.GetTaskDaySummaries
{
    // editbycodex: rango inclusivo de dias visibles solicitado por el calendario.
    public sealed class GetTaskDaySummariesQuery
    {
        public DateTime StartDate { get; init; }
        public DateTime EndDate { get; init; }
    }
}
