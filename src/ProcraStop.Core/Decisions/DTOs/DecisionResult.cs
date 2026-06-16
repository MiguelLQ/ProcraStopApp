namespace ProcraStop.Core.Decisions.DTOs
{
    public class DecisionResult
    {
        // mensaje que se muestra en el dashboard para explicar la decision tomada por el motor de decisiones
        public string Message { get; set; } = string.Empty;
        // tipo de decision tomada por el motor de decisiones, puede ser una recomendacion,
        // una advertencia o una accion a tomar
        public DecisionType Type { get; set; }
        // accion sugerida por el motor de decisiones, puede ser un link a una pagina de ayuda,
        // un video explicativo o una tarea a realizar
        public string? SuggestedAction { get; set; }
        // motivo de la decision tomada por el motor de decisiones, puede ser una regla que
        // se cumplio o una condicion que se detecto
        public string? Reason { get; set; }
        //  NUEVO
        public double Score { get; set; }
        // categoria de la decision tomada por el motor de decisiones, puede ser relacionada con la tarea actual,
        public DecisionCategory Category { get; init; }

        // NUEVO (opcional, UX avanzado)
        // lista de insights generados por el motor de decisiones, pueden ser recomendaciones,
        // advertencias o acciones a tomar
        public List<string>? TaskInsights { get; set; }
        public List<string>? HabitInsights { get; set; }
        // 
        public List<string>? SecondaryInsights { get; set; }
        public List<string>? Insights { get; set; }

    }
}
