using ProcraStop.Application.Tasks.Analytics;

namespace ProcraStop.Modules.Tasks.Presentation.Models
{
    // D1 EPA: modelo de presentacion con textos ya formateados para la lista por curso.
    public sealed class CourseMetricItemModel
    {
        public string CourseName { get; init; } = "Sin curso";
        public string AppraisalText { get; init; } = "Sin valoración declarada";
        public string VolumeText { get; init; } = string.Empty;
        public string DelayText { get; init; } = "Retraso: sin datos";
        public string OnTimeText { get; init; } = "Inicio puntual: sin datos";
        public string LastMinuteText { get; init; } = "Al límite: sin datos";
        public string AvoidanceText { get; init; } = string.Empty;
        public bool ShowAvoidance { get; init; }
        public bool ShowMetrics { get; init; }
        public bool ShowInsufficientData => !ShowMetrics;

        public static CourseMetricItemModel FromDto(CourseProcrastinationMetricDto dto, bool readingsOnly)
        {
            return new CourseMetricItemModel
            {
                CourseName = dto.CourseName,
                AppraisalText = BuildAppraisalText(dto),
                VolumeText = readingsOnly
                    ? $"{dto.TotalTasks} lectura(s)"
                    : $"{dto.TotalTasks} tarea(s) · {dto.TotalReadings} lectura(s)",
                DelayText = BuildDelayText(dto),
                OnTimeText = dto.MeasuredStarts == 0
                    ? "Inicio puntual: sin datos"
                    : $"{dto.OnTimeStartRate:P0} inicio puntual",
                LastMinuteText = dto.MeasuredDueStarts == 0
                    ? "Al límite: sin datos"
                    : $"{dto.LastMinuteStartRate:P0} al límite",
                AvoidanceText = $"{dto.OverdueStartNotStarted} sin iniciar",
                ShowAvoidance = dto.OverdueStartNotStarted > 0,
                ShowMetrics = dto.HasEnoughData
            };
        }

        private static string BuildAppraisalText(CourseProcrastinationMetricDto dto)
        {
            var parts = new List<string>();
            if (dto.InterestLevel.HasValue)
                parts.Add($"Interés {dto.InterestLevel}/5");
            if (dto.DifficultyLevel.HasValue)
                parts.Add($"Dificultad {dto.DifficultyLevel}/5");

            return parts.Count == 0
                ? "Sin valoración declarada"
                : string.Join(" · ", parts);
        }

        private static string BuildDelayText(CourseProcrastinationMetricDto dto)
        {
            if (!dto.AvgStartDelayDays.HasValue)
                return "Retraso: sin datos";

            var value = Math.Round(dto.AvgStartDelayDays.Value, 1);
            if (Math.Abs(value) < 0.1)
                return "Retraso: a tiempo";

            return value > 0
                ? $"Retraso: +{value:F1} día(s)"
                : $"Retraso: {value:F1} día(s)";
        }
    }
}
