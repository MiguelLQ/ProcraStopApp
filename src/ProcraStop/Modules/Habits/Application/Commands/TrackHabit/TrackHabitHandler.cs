
using ProcraStop.Modules.Habits.Application.Abstractions;
using ProcraStop.Modules.Habits.Domain.Entities;

namespace ProcraStop.Modules.Habits.Application.Commands.TrackHabit
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

            await _logRepository.AddAsync(log);
        }
    }
}
