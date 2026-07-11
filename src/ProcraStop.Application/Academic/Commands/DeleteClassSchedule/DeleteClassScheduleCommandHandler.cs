using ProcraStop.Application.Academic.Abstractions;

namespace ProcraStop.Application.Academic.Commands.DeleteClassSchedule;

public sealed class DeleteClassScheduleCommandHandler
{
    private readonly IClassScheduleRepository _repository;

    public DeleteClassScheduleCommandHandler(IClassScheduleRepository repository)
    {
        _repository = repository;
    }

    public Task HandleAsync(DeleteClassScheduleCommand command, CancellationToken ct = default)
        => _repository.DeleteAsync(command.Id, ct);
}
