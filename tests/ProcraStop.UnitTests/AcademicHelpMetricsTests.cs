using ProcraStop.Application.StudyTechniques.Services;
using ProcraStop.Domain.StudyTechniques.Entities;
using ProcraStop.Domain.StudyTechniques.Enums;

namespace ProcraStop.UnitTests;

public sealed class AcademicHelpMetricsTests
{
    private static readonly DateTime Now = new(2026, 11, 9, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Build_calculates_request_and_resolve_times()
    {
        var requests = new[]
        {
            // Creada 3 dias antes, pedida a las 36 h, resuelta 12 h despues.
            Request(HelpRequestStatus.Resolved,
                created: Now.AddDays(-3),
                requested: Now.AddHours(-36),
                resolved: Now.AddHours(-24)),

            // Creada 2 dias antes, pedida 1.5 dias despues, aun sin resolver.
            Request(HelpRequestStatus.Requested,
                created: Now.AddDays(-2),
                requested: Now.AddHours(-12),
                resolved: null),

            // Pendiente estancada: 4 dias sin realizarse.
            Request(HelpRequestStatus.Planned,
                created: Now.AddDays(-4),
                requested: null,
                resolved: null),

            // Pendiente reciente: no debe contar como estancada.
            Request(HelpRequestStatus.Planned,
                created: Now.AddDays(-1),
                requested: null,
                resolved: null)
        };

        var metrics = AcademicHelpMetricsService.Build(requests, Now);

        Assert.Equal(4, metrics.Total);
        Assert.Equal(2, metrics.Planned);
        Assert.Equal(1, metrics.Requested);
        Assert.Equal(1, metrics.Resolved);
        Assert.Equal(36, metrics.AverageHoursToRequest!.Value, precision: 3);
        Assert.Equal(12, metrics.AverageHoursToResolve!.Value, precision: 3);
        Assert.Equal(1, metrics.PendingFollowUp);
        Assert.Equal(4, metrics.OldestPendingDays);
        Assert.True(metrics.HasMetrics);
        Assert.True(metrics.HasRequestSamples);
        Assert.True(metrics.HasResolveSamples);
        Assert.True(metrics.HasPendingFollowUp);
    }

    [Fact]
    public void Build_without_requests_returns_empty_metrics()
    {
        var metrics = AcademicHelpMetricsService.Build(Array.Empty<AcademicHelpRequest>(), Now);

        Assert.Equal(0, metrics.Total);
        Assert.Null(metrics.AverageHoursToRequest);
        Assert.Null(metrics.AverageHoursToResolve);
        Assert.Null(metrics.OldestPendingDays);
        Assert.False(metrics.HasMetrics);
        Assert.False(metrics.HasPendingFollowUp);
    }

    private static AcademicHelpRequest Request(
        HelpRequestStatus status,
        DateTime created,
        DateTime? requested,
        DateTime? resolved) =>
        AcademicHelpRequest.Rehydrate(
            Guid.NewGuid(), null, null, "Tema", "¿Qué no entiendo?",
            HelpChannel.Teacher, null, status, created, requested, resolved);
}
