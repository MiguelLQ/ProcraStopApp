using Microsoft.EntityFrameworkCore;
using ProcraStop.Application.StudyTechniques.Abstractions;
using ProcraStop.Domain.StudyTechniques.Entities;
using ProcraStop.Infrastructure.DBContext;
using ProcraStop.Infrastructure.StudyTechniques.Persistence.Mappings;

namespace ProcraStop.Infrastructure.StudyTechniques.Repositories;

public sealed class StudyTechniqueSessionRepository : IStudyTechniqueSessionRepository
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

    public StudyTechniqueSessionRepository(IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task AddAsync(StudyTechniqueSession session, CancellationToken ct)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync(ct);
        context.StudyTechniqueSessions.Add(StudyTechniqueSessionMapper.ToEntity(session));
        await context.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<StudyTechniqueSession>> GetRecentAsync(int days, CancellationToken ct)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync(ct);
        var cutoff = DateTime.UtcNow.AddDays(-Math.Max(1, days));
        var entities = await context.StudyTechniqueSessions
            .AsNoTracking()
            .Where(x => x.StartedAtUtc >= cutoff)
            .OrderByDescending(x => x.StartedAtUtc)
            .ToListAsync(ct);

        return entities.Select(StudyTechniqueSessionMapper.ToDomain).ToArray();
    }
}
