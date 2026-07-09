

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
            return await _repository.GetByIdAsync(query.HabitId, CancellationToken.None); // editbycodex: búsqueda indexada por clave.
        }
    }
}
