
using ProcraStop.Core.Decisions.DTOs;

namespace ProcraStop.Core.Decisions.Mappers
{
    public static class DecisionMapper
    {
        public static DecisionViewModel ToViewModel(DecisionResult result)
        {
            return new DecisionViewModel
            {
                Title = MapTitle(result.Type),
                Message = result.Message,
                Reason = result.Reason,
                SuggestedAction = result.SuggestedAction,
                Severity = result.Type.ToString(),
                Insights = result.TaskInsights ?? new(),
                SupportingInsights = result.SecondaryInsights ?? new(),
                Icon = MapIcon(result.Type),
                Color = MapColor(result.Type)
            };
        }

        private static string MapTitle(DecisionType type)
        {
            return type switch
            {
                DecisionType.Success => "Buen trabajo",
                DecisionType.Warning => "Atención",
                DecisionType.Critical => "Acción necesaria",
                _ => "Información"
            };
        }

        private static string MapIcon(DecisionType type)
        {
            return type switch
            {
                DecisionType.Success => "check",
                DecisionType.Warning => "warning",
                DecisionType.Critical => "alert",
                _ => "info"
            };
        }

        private static string MapColor(DecisionType type)
        {
            return type switch
            {
                DecisionType.Success => "green",
                DecisionType.Warning => "orange",
                DecisionType.Critical => "red",
                _ => "blue"
            };
        }
    }
}
