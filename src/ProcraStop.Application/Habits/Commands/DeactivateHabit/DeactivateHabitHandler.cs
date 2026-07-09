using ProcraStop.Application.Habits.Abstractions;

namespace ProcraStop.Application.Habits.Commands.DeactivateHabit;

public sealed class DeactivateHabitHandler
{
    private readonly IHabitRepository _repository;

    public DeactivateHabitHandler(IHabitRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(DeactivateHabitCommand command, CancellationToken ct = default)
    {
        var habit = await _repository.GetByIdAsync(command.HabitId, ct)
            ?? throw new InvalidOperationException("Hábito no encontrado.");
        habit.Deactivate();
        await _repository.UpdateAsync(habit, ct); // editbycodex: pausa reversible en dominio, sin borrar historial.
    }
}
