using ProcraStop.Application.StudyTechniques.Abstractions;
using ProcraStop.Application.StudyTechniques.DTOs;
using ProcraStop.Domain.StudyTechniques.Entities;
using ProcraStop.Domain.StudyTechniques.Enums;

namespace ProcraStop.Application.StudyTechniques.Services;

// D2 EPA: tiempo hasta pedir ayuda y tiempo hasta resolver.
// No cambia el modelo de datos: usa CreatedAtUtc, RequestedAtUtc y ResolvedAtUtc.
public sealed class AcademicHelpMetricsService
{
    private readonly IAcademicHelpRepository _repository;

    public AcademicHelpMetricsService(IAcademicHelpRepository repository)
    {
        _repository = repository;
    }

    public async Task<AcademicHelpMetricsDto> GetAsync(CancellationToken ct = default)
    {
        var requests = await _repository.GetRecentAsync(ct);
        return Build(requests, DateTime.UtcNow);
    }

    public static AcademicHelpMetricsDto Build(
        IReadOnlyList<AcademicHelpRequest> requests,
        DateTime nowUtc)
    {
        var requestDurations = requests
            .Where(r => r.RequestedAtUtc.HasValue)
            .Select(r => (r.RequestedAtUtc!.Value - r.CreatedAtUtc).TotalHours)
            .Where(hours => hours >= 0)
            .ToArray();

        var resolveDurations = requests
            .Where(r => r.RequestedAtUtc.HasValue && r.ResolvedAtUtc.HasValue)
            .Select(r => (r.ResolvedAtUtc!.Value - r.RequestedAtUtc!.Value).TotalHours)
            .Where(hours => hours >= 0)
            .ToArray();

        var pending = requests
            .Where(r => r.Status == HelpRequestStatus.Planned)
            .ToArray();

        var pendingAges = pending
            .Select(r => (nowUtc - r.CreatedAtUtc).TotalDays)
            .Where(days => days >= 0)
            .ToArray();

        return new AcademicHelpMetricsDto
        {
            Total = requests.Count,
            Planned = pending.Length,
            Requested = requests.Count(r => r.Status == HelpRequestStatus.Requested),
            Resolved = requests.Count(r => r.Status == HelpRequestStatus.Resolved),
            AverageHoursToRequest = requestDurations.Length == 0
                ? null
                : requestDurations.Average(),
            AverageHoursToResolve = resolveDurations.Length == 0
                ? null
                : resolveDurations.Average(),
            PendingFollowUp = pendingAges.Count(days => days >= AcademicHelpMetricsDto.FollowUpThresholdDays),
            OldestPendingDays = pendingAges.Length == 0
                ? null
                : (int)Math.Floor(pendingAges.Max())
        };
    }
}
