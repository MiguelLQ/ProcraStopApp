using ProcraStop.Domain.Tasks.Entities;

namespace ProcraStop.Application.Tasks.Services
{
    // editbycodex: D1 EPA "postergacion al ultimo momento".
    // PlannedStartDate es una fecha sin hora; el aviso se programa a primera hora
    // y, si la fecha ya paso, se emite en el siguiente ciclo corto para no perder la senal.
    public static class TaskStartReminderPolicy
    {
        private const int PreferredStartHour = 8;
        private static readonly TimeSpan AlreadyLateDelay = TimeSpan.FromMinutes(2);

        public static DateTime? ResolveNotificationTime(StudyTask task, DateTime now)
        {
            if (task.StartedAtUtc is not null || task.PlannedStartDate is not { } plannedStart)
                return null;

            var preferred = plannedStart.Date.AddHours(PreferredStartHour);
            if (preferred > now)
                return preferred;

            // La fecha de inicio ya llego y la tarea sigue sin empezar.
            return now.Add(AlreadyLateDelay);
        }

        public static bool ShouldWarn(StudyTask task, DateTime today) =>
            task.StartedAtUtc is null &&
            !task.IsCompleted &&
            !task.IsArchived &&
            task.PlannedStartDate.HasValue &&
            task.PlannedStartDate.Value.Date <= today;
    }
}
