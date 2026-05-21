

using ProcraStop.Application.Habits.Abstractions;
using ProcraStop.Domain.Habits.Entities;

namespace ProcraStop.Application.Habits.Commands.TrackHabit
{
    public class TrackHabitHandler
    {
        private readonly IHabitLogRepository _logRepository;

        public TrackHabitHandler(IHabitLogRepository logRepository)
        {
            _logRepository = logRepository;
        }

        public async Task Handle(TrackHabitCommand command)
        {
            var log = HabitLog.Create(
                command.HabitId,
                command.Date,
                command.Value,
                command.Notes);

            await _logRepository.UpsertAsync(log);
        }
    }
}
