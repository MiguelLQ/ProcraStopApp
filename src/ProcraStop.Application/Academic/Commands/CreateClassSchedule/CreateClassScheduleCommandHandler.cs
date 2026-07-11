using ProcraStop.Application.Academic.Abstractions;
using ProcraStop.Domain.Academic.Entities;

namespace ProcraStop.Application.Academic.Commands.CreateClassSchedule;

public sealed class CreateClassScheduleCommandHandler
{
    private readonly IClassScheduleRepository _repository;

    public CreateClassScheduleCommandHandler(IClassScheduleRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> HandleAsync(CreateClassScheduleCommand command, CancellationToken ct = default)
    {
        var schedule = ClassSchedule.Create(
            command.CourseId,
            command.DayOfWeek,
            command.StartTime,
            command.EndTime,
            command.Classroom,
            command.Instructor,
            command.ReminderMinutesBefore);

        await _repository.AddAsync(schedule, ct);

        return schedule.Id;
    }
}
