namespace ProcraStop.Application.Tasks.DTOs;

public sealed class PlannedStepDto
{
    public string Title { get; init; } = string.Empty;
    public DateTime? PlannedDate { get; init; }
}
