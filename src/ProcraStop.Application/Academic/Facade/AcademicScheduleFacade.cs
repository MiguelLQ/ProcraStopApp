using ProcraStop.Application.Academic.Commands.CreateClassSchedule;
using ProcraStop.Application.Academic.Commands.DeleteClassSchedule;
using ProcraStop.Application.Academic.Commands.UpdateClassSchedule;
using ProcraStop.Application.Academic.DTOs;
using ProcraStop.Application.Academic.Queries.GetClassSchedules;

namespace ProcraStop.Application.Academic.Facade;

public sealed class AcademicScheduleFacade : IAcademicScheduleFacade
{
    private readonly GetClassSchedulesQueryHandler _getHandler;
    private readonly CreateClassScheduleCommandHandler _createHandler;
    private readonly UpdateClassScheduleCommandHandler _updateHandler;
    private readonly DeleteClassScheduleCommandHandler _deleteHandler;

    public AcademicScheduleFacade(
        GetClassSchedulesQueryHandler getHandler,
        CreateClassScheduleCommandHandler createHandler,
        UpdateClassScheduleCommandHandler updateHandler,
        DeleteClassScheduleCommandHandler deleteHandler)
    {
        _getHandler = getHandler;
        _createHandler = createHandler;
        _updateHandler = updateHandler;
        _deleteHandler = deleteHandler;
    }

    public Task<IReadOnlyList<ClassScheduleDto>> GetScheduleAsync(DayOfWeek? dayOfWeek = null, CancellationToken ct = default)
        => _getHandler.HandleAsync(new GetClassSchedulesQuery { DayOfWeek = dayOfWeek }, ct);

    public Task<Guid> CreateScheduleAsync(CreateClassScheduleRequest request, CancellationToken ct = default)
    {
        return _createHandler.HandleAsync(new CreateClassScheduleCommand
        {
            CourseId = request.CourseId,
            DayOfWeek = request.DayOfWeek,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Classroom = request.Classroom,
            Instructor = request.Instructor,
            ReminderMinutesBefore = request.ReminderMinutesBefore
        }, ct);
    }

    public Task UpdateScheduleAsync(UpdateClassScheduleRequest request, CancellationToken ct = default)
    {
        return _updateHandler.HandleAsync(new UpdateClassScheduleCommand
        {
            Id = request.Id,
            CourseId = request.CourseId,
            DayOfWeek = request.DayOfWeek,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Classroom = request.Classroom,
            Instructor = request.Instructor,
            ReminderMinutesBefore = request.ReminderMinutesBefore
        }, ct);
    }

    public Task DeleteScheduleAsync(Guid id, CancellationToken ct = default)
        => _deleteHandler.HandleAsync(new DeleteClassScheduleCommand { Id = id }, ct);
}
