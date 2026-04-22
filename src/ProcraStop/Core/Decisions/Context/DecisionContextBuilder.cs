using ProcraStop.Modules.Habits.Application.DTOs;
using ProcraStop.Modules.Habits.Domain.Entities;
using ProcraStop.Modules.Tasks.Application.EngineTask.Signals;

namespace ProcraStop.Core.Decisions.Context
{
    public class DecisionContextBuilder
    {
     
            public DecisionContext Build(
                TaskSignals taskSignals,
                HabitSignals habitSignals)
            {
                return new DecisionContext
                {
                    TaskSignals = taskSignals,
                    Habits = habitSignals ?? HabitSignals.Empty()
                };
            }
    }
}
