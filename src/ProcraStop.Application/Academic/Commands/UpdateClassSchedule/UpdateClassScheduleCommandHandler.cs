using ProcraStop.Application.Academic.Abstractions;

namespace ProcraStop.Application.Academic.Commands.UpdateClassSchedule;

public sealed class UpdateClassScheduleCommandHandler
{
    private readonly IClassScheduleRepository _repository;

    public UpdateClassScheduleCommandHandler(IClassScheduleRepository repository)
    {
        _repository = repository;
    }

    public async Task HandleAsync(UpdateClassScheduleCommand command, CancellationToken ct = default)
    {
        var schedule = await _repository.GetByIdAsync(command.Id, ct);
        if (schedule is null)
            throw new InvalidOperationException("Horario no encontrado.");

        schedule.Update(
            command.CourseId,
            command.DayOfWeek,
            command.StartTime,
            command.EndTime,
            command.Classroom,
            command.Instructor,
            command.ReminderMinutesBefore);

        await _repository.UpdateAsync(schedule, ct);
    }
}
