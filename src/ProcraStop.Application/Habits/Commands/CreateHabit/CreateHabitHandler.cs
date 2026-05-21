
using ProcraStop.Application.Habits.Abstractions;
using ProcraStop.Domain.Habits.Entities;
using ProcraStop.Domain.Habits.ValueObjects;

namespace ProcraStop.Application.Habits.Commands.CreateHabit
{
    public class CreateHabitHandler
    {
        private readonly IHabitRepository _repository;

        public CreateHabitHandler(IHabitRepository repository)
        {
            _repository = repository;
        }

        public async Task Handle(CreateHabitCommand command)
        {
            var target = HabitTarget.Create(command.TargetValue);

            var days = command.DaysOfWeek
            .Select(d => (DayOfWeek)d)
            .ToList();

            var schedule = HabitSchedule.Create(days);

            var rule = HabitCompletionRule.Create(command.RequiredPercentage);

            var habit = Habit.Create(
                command.Name,
                command.Unit,
                target,
                schedule,
                rule,
                command.TrackingType,
                command.Weight);

            await _repository.AddAsync(habit);
        }
    }
}
