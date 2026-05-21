using ProcraStop.Core.Decisions.Context;
using ProcraStop.Core.Decisions.DTOs;
using ProcraStop.Core.Decisions.Signals;
namespace ProcraStop.Core.Decisions.Rules
{
    public interface IDecisionRule
    {
        // La prioridad de la regla, donde un número menor indica una mayor prioridad
        int Priority { get; }
        DecisionCategory Category { get; } // NUEVO

        // Método para verificar si la regla se aplica al contexto actual
        bool IsMatch(DecisionContext context);
        // Método para crear una decisión basada en el contexto actual
        //DecisionResult CreateDecision(DecisionContext context);
        // NUEVO (opcional, no rompe nada)
        DecisionResult CreateDecision(
            DecisionContext context,
            IReadOnlyList<UnifiedSignal> signals);
    }
}
