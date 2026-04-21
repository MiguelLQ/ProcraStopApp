namespace ProcraStop.Modules.Tasks.Application.EngineTask.DTOs
{
    public class DashboardDecisionDto
    {
        public string Message { get; set; } = string.Empty;
        public GuidanceType Type { get; set; }
        public List<string>? SecondaryInsights { get; set; } = new();
        public string? SuggestedAction { get; set; }
        // motivo de la decision tomada por el motor de decisiones, puede ser una regla que
        // se cumplio o una condicion que se detecto
        public string? Reason { get; set; }

        // NUEVO (opcional, UX avanzado)
        // lista de insights generados por el motor de decisiones, pueden ser recomendaciones,
        // advertencias o acciones a tomar
        public List<string>? Insights { get; set; }
        // 

    }
}
