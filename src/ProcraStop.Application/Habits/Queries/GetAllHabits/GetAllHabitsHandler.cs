

using ProcraStop.Application.Habits.Abstractions;
using ProcraStop.Domain.Habits.Entities;

namespace ProcraStop.Application.Habits.Queries.GetAllHabits
{
    public class GetAllHabitsHandler
    {
        private readonly IHabitRepository _repository;

        public GetAllHabitsHandler(IHabitRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<Habit>> Handle(GetAllHabitsQuery query)
        {
            return await _repository.GetAllAsync();
        }
    }
}
