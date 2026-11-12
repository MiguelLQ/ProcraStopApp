namespace ProcraStop.Application.StudyTechniques.DTOs;

// D2 EPA: metricas conductuales de busqueda de ayuda.
// Se calculan con los timestamps que ya existen en AcademicHelpRequest.
public sealed class AcademicHelpMetricsDto
{
    public const int FollowUpThresholdDays = 2;

    public int Total { get; init; }
    public int Planned { get; init; }
    public int Requested { get; init; }
    public int Resolved { get; init; }

    public double? AverageHoursToRequest { get; init; }
    public double? AverageHoursToResolve { get; init; }

    public int PendingFollowUp { get; init; }
    public int? OldestPendingDays { get; init; }

    public bool HasMetrics => Total > 0;
    public bool HasRequestSamples => AverageHoursToRequest.HasValue;
    public bool HasResolveSamples => AverageHoursToResolve.HasValue;
    public bool HasPendingFollowUp => PendingFollowUp > 0;
}
