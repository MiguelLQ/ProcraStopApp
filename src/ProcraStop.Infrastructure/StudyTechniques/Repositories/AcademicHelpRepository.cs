using Microsoft.EntityFrameworkCore;
using ProcraStop.Application.StudyTechniques.Abstractions;
using ProcraStop.Domain.StudyTechniques.Entities;
using ProcraStop.Domain.StudyTechniques.Enums;
using ProcraStop.Infrastructure.DBContext;
using ProcraStop.Infrastructure.StudyTechniques.Persistence.Entities;

namespace ProcraStop.Infrastructure.StudyTechniques.Repositories;

internal sealed class AcademicHelpRepository : IAcademicHelpRepository
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public AcademicHelpRepository(IDbContextFactory<AppDbContext> factory) => _factory = factory;

    public async Task AddAsync(AcademicHelpRequest request, CancellationToken ct)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        db.AcademicHelpRequests.Add(ToEntity(request));
        await db.SaveChangesAsync(ct);
    }

    public async Task<AcademicHelpRequest?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        var entity = await db.AcademicHelpRequests.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        return entity is null ? null : ToDomain(entity);
    }

    public async Task<IReadOnlyList<AcademicHelpRequest>> GetRecentAsync(CancellationToken ct)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        var entities = await db.AcademicHelpRequests.AsNoTracking()
            .OrderBy(x => x.Status == (int)HelpRequestStatus.Resolved)
            .ThenByDescending(x => x.CreatedAtUtc)
            .Take(50)
            .ToListAsync(ct);
        return entities.Select(ToDomain).ToArray();
    }

    public async Task UpdateAsync(AcademicHelpRequest request, CancellationToken ct)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        var entity = await db.AcademicHelpRequests.FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new InvalidOperationException("Consulta no encontrada.");
        entity.Status = (int)request.Status;
        entity.RequestedAtUtc = request.RequestedAtUtc;
        entity.ResolvedAtUtc = request.ResolvedAtUtc;
        await db.SaveChangesAsync(ct);
    }

    private static AcademicHelpRequestEntity ToEntity(AcademicHelpRequest x) => new()
    {
        Id = x.Id,
        TaskId = x.TaskId,
        CourseId = x.CourseId,
        Topic = x.Topic,
        Question = x.Question,
        Channel = (int)x.Channel,
        ContactName = x.ContactName,
        Status = (int)x.Status,
        CreatedAtUtc = x.CreatedAtUtc,
        RequestedAtUtc = x.RequestedAtUtc,
        ResolvedAtUtc = x.ResolvedAtUtc
    };

    private static AcademicHelpRequest ToDomain(AcademicHelpRequestEntity x) =>
        AcademicHelpRequest.Rehydrate(
            x.Id, x.TaskId, x.CourseId, x.Topic, x.Question,
            (HelpChannel)x.Channel, x.ContactName, (HelpRequestStatus)x.Status,
            x.CreatedAtUtc, x.RequestedAtUtc, x.ResolvedAtUtc);
}
