
using ProcraStop.Modules.Habits.Application.Abstractions;
using ProcraStop.Modules.Habits.Domain.Entities;

namespace ProcraStop.Modules.Habits.Application.Queries.GetAllHabits
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
