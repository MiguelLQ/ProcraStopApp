using ProcraStop.Application.StudyTechniques.Abstractions;
using ProcraStop.Domain.StudyTechniques.Entities;
using ProcraStop.Domain.StudyTechniques.Enums;

namespace ProcraStop.Application.StudyTechniques.Services;

public sealed class AcademicHelpService
{
    private readonly IAcademicHelpRepository _repository;

    public AcademicHelpService(IAcademicHelpRepository repository) => _repository = repository;

    public async Task<Guid> PlanAsync(
        Guid? taskId, Guid? courseId, string topic, string question,
        HelpChannel channel, string? contactName, CancellationToken ct = default)
    {
        var request = AcademicHelpRequest.Create(
            taskId, courseId, topic, question, channel, contactName);
        await _repository.AddAsync(request, ct);
        return request.Id;
    }

    public Task<IReadOnlyList<AcademicHelpRequest>> GetRecentAsync(CancellationToken ct = default)
        => _repository.GetRecentAsync(ct);

    public async Task MarkRequestedAsync(Guid id, CancellationToken ct = default)
    {
        var request = await _repository.GetByIdAsync(id, ct)
            ?? throw new InvalidOperationException("Consulta no encontrada.");
        request.MarkRequested();
        await _repository.UpdateAsync(request, ct);
    }

    public async Task MarkResolvedAsync(Guid id, CancellationToken ct = default)
    {
        var request = await _repository.GetByIdAsync(id, ct)
            ?? throw new InvalidOperationException("Consulta no encontrada.");
        request.MarkResolved();
        await _repository.UpdateAsync(request, ct);
    }
}
