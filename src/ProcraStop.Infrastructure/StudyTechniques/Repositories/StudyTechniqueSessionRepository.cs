using Microsoft.EntityFrameworkCore;
using ProcraStop.Application.StudyTechniques.Abstractions;
using ProcraStop.Domain.StudyTechniques.Entities;
using ProcraStop.Infrastructure.DBContext;
using ProcraStop.Infrastructure.StudyTechniques.Persistence.Mappings;

namespace ProcraStop.Infrastructure.StudyTechniques.Repositories;

public sealed class StudyTechniqueSessionRepository : IStudyTechniqueSessionRepository
{
    private readonly AppDbContext _context;

    public StudyTechniqueSessionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(StudyTechniqueSession session, CancellationToken ct)
    {
        _context.StudyTechniqueSessions.Add(StudyTechniqueSessionMapper.ToEntity(session));
        await _context.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<StudyTechniqueSession>> GetRecentAsync(int days, CancellationToken ct)
    {
        var cutoff = DateTime.UtcNow.AddDays(-Math.Max(1, days));
        var entities = await _context.StudyTechniqueSessions
            .Where(x => x.StartedAtUtc >= cutoff)
            .OrderByDescending(x => x.StartedAtUtc)
            .ToListAsync(ct);

        return entities.Select(StudyTechniqueSessionMapper.ToDomain).ToArray();
    }
}
