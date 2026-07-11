using ProcraStop.Domain.Academic.Entities;
using ProcraStop.Infrastructure.Academic.Persistence.Entities;

namespace ProcraStop.Infrastructure.Academic.Persistence.Mappings;

public static class ClassScheduleMappings
{
    public static ClassScheduleEntity ToEntity(this ClassSchedule schedule)
    {
        return new ClassScheduleEntity
        {
            Id = schedule.Id,
            CourseId = schedule.CourseId,
            DayOfWeek = (int)schedule.DayOfWeek,
            StartTicks = schedule.StartTime.Ticks,
            EndTicks = schedule.EndTime.Ticks,
            Classroom = schedule.Classroom,
            Instructor = schedule.Instructor,
            ReminderMinutesBefore = schedule.ReminderMinutesBefore,
            IsActive = schedule.IsActive,
            CreatedAtUtc = schedule.CreatedAtUtc
        };
    }

    public static ClassSchedule ToDomain(this ClassScheduleEntity entity)
    {
        return ClassSchedule.Restore(
            entity.Id,
            entity.CourseId,
            (DayOfWeek)entity.DayOfWeek,
            TimeSpan.FromTicks(entity.StartTicks),
            TimeSpan.FromTicks(entity.EndTicks),
            entity.Classroom,
            entity.Instructor,
            entity.ReminderMinutesBefore,
            entity.IsActive,
            entity.CreatedAtUtc);
    }
}
