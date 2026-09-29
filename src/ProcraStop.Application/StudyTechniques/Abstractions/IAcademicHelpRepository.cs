using ProcraStop.Domain.StudyTechniques.Entities;

namespace ProcraStop.Application.StudyTechniques.Abstractions;

public interface IAcademicHelpRepository
{
    Task AddAsync(AcademicHelpRequest request, CancellationToken ct);
    Task<AcademicHelpRequest?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<AcademicHelpRequest>> GetRecentAsync(CancellationToken ct);
    Task UpdateAsync(AcademicHelpRequest request, CancellationToken ct);
}
