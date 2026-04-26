

using ProcraStop.Modules.Dashboard.Application.Models;

namespace ProcraStop.Modules.Dashboard.Application.DTOs
{
    public static class DashboardMapper
    {
        public static DashboardDecisionDto ToDto(DashboardReadModel model)
        {
            return new DashboardDecisionDto
            {
                CoreState = new CoreDecisionDto
                {
                    Message = model.CoreState.Message,
                    Type = model.CoreState.Type,
                    Reason = model.CoreState.Reason,
                    SuggestedAction = model.CoreState.SuggestedAction
                },

                Signals = model.Signals.Select(s => new SignalDto
                {
                    Name = s.Name,
                    Category = s.Category,
                    Score = s.Score,
                    Type = s.Type
                }).ToList(),

                TaskInsights = model.TaskInsights,
                HabitInsights = model.HabitInsights,
                StressInsights = model.StressInsights
            };
        }
    }
}
