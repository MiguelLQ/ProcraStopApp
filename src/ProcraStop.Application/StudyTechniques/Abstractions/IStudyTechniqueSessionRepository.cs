using ProcraStop.Domain.StudyTechniques.Entities;

namespace ProcraStop.Application.StudyTechniques.Abstractions;

public interface IStudyTechniqueSessionRepository
{
    Task AddAsync(StudyTechniqueSession session, CancellationToken ct);
    Task<IReadOnlyList<StudyTechniqueSession>> GetRecentAsync(int days, CancellationToken ct);
    Task<IReadOnlyList<StudyTechniqueSession>> GetByTaskIdAsync(Guid taskId, CancellationToken ct);
}
