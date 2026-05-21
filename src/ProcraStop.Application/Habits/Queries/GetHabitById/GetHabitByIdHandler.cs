

using ProcraStop.Application.Habits.Abstractions;
using ProcraStop.Domain.Habits.Entities;

namespace ProcraStop.Application.Habits.Queries.GetHabitById
{
    public class GetHabitByIdHandler
    {
        private readonly IHabitRepository _repository;

        public GetHabitByIdHandler(IHabitRepository repository)
        {
            _repository = repository;
        }

        public async Task<Habit?> Handle(GetHabitByIdQuery query)
        {
            var habits = await _repository.GetAllAsync();
            foreach (var h in habits)
            {
                Console.WriteLine($"🧩 DB HabitId: {h.Id}");
            }

            Console.WriteLine($"🔍 Buscando: {query.HabitId}");

            return habits.FirstOrDefault(h => h.Id == query.HabitId);
        }
    }
}
