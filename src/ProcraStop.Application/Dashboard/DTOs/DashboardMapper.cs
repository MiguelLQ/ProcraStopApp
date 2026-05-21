


using ProcraStop.Application.Dashboard.Models;

namespace ProcraStop.Application.Dashboard.DTOs
{
    public static class DashboardMapper
    {
        public static DashboardDecisionDto ToDto(DashboardReadModel model)
        {
            return new DashboardDecisionDto
            {
                CoreState = model.CoreState != null ? new CoreDecisionDto
                {
                    Message = model.CoreState.Message,
                    Type = model.CoreState.Type,
                    Reason = model.CoreState.Reason,
                    SuggestedAction = model.CoreState.SuggestedAction
                } : null,

                Signals = model.Signals?.Select(s => new SignalDto
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
