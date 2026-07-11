using System.Globalization;
using ProcraStop.Application.Academic.Abstractions;
using ProcraStop.Application.Academic.DTOs;

namespace ProcraStop.Application.Academic.Queries.GetClassSchedules;

public sealed class GetClassSchedulesQueryHandler
{
    private readonly IClassScheduleRepository _scheduleRepository;
    private readonly ICourseRepository _courseRepository;

    public GetClassSchedulesQueryHandler(
        IClassScheduleRepository scheduleRepository,
        ICourseRepository courseRepository)
    {
        _scheduleRepository = scheduleRepository;
        _courseRepository = courseRepository;
    }

    public async Task<IReadOnlyList<ClassScheduleDto>> HandleAsync(
        GetClassSchedulesQuery query,
        CancellationToken ct = default)
    {
        var schedules = query.DayOfWeek.HasValue
            ? await _scheduleRepository.GetByDayAsync(query.DayOfWeek.Value, ct)
            : await _scheduleRepository.GetAllActiveAsync(ct);

        var courses = await _courseRepository.GetAllAsync(ct);
        var coursesById = courses.ToDictionary(c => c.Id);

        return schedules
            .OrderBy(s => ((int)s.DayOfWeek + 6) % 7)
            .ThenBy(s => s.StartTime)
            .Select(s =>
            {
                coursesById.TryGetValue(s.CourseId, out var course);
                return new ClassScheduleDto
                {
                    Id = s.Id,
                    CourseId = s.CourseId,
                    CourseName = course?.Name ?? "Curso eliminado",
                    CourseColor = course?.Color ?? "#4F8CFF",
                    DayOfWeek = s.DayOfWeek,
                    DayName = ToDayName(s.DayOfWeek),
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    TimeRangeText = $"{FormatTime(s.StartTime)} - {FormatTime(s.EndTime)}",
                    Classroom = s.Classroom,
                    Instructor = s.Instructor,
                    ReminderMinutesBefore = s.ReminderMinutesBefore
                };
            })
            .ToList();
    }

    private static string ToDayName(DayOfWeek day)
    {
        var reference = DateTime.Today;
        var offset = ((int)day - (int)reference.DayOfWeek + 7) % 7;
        return reference.AddDays(offset).ToString("dddd", new CultureInfo("es-ES"));
    }

    private static string FormatTime(TimeSpan value)
        => DateTime.Today.Add(value).ToString("HH:mm", CultureInfo.InvariantCulture);
}
