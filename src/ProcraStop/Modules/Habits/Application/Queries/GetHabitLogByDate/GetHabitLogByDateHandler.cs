
using ProcraStop.Modules.Habits.Application.Abstractions;
using ProcraStop.Modules.Habits.Domain.Entities;

namespace ProcraStop.Modules.Habits.Application.Queries.GetHabitLogByDate
{
    public class GetHabitLogByDateHandler
    {
        private readonly IHabitLogRepository _repository;

        public GetHabitLogByDateHandler(IHabitLogRepository repository)
        {
            _repository = repository;
        }

        public async Task<HabitLog?> Handle(GetHabitLogByDateQuery query)
        {
            return await _repository.GetByHabitAndDateAsync(
                query.HabitId,
                query.Date);
        }
    }
}
