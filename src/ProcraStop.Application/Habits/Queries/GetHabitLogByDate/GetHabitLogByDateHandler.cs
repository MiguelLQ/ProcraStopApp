

using ProcraStop.Application.Habits.Abstractions;
using ProcraStop.Domain.Habits.Entities;

namespace ProcraStop.Application.Habits.Queries.GetHabitLogByDate
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
