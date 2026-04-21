using ProcraStop.Core.Decisions.Context;
using ProcraStop.Core.Decisions.DTOs;
namespace ProcraStop.Core.Decisions.Rules
{
    public interface IDecisionRule
    {
        // La prioridad de la regla, donde un número menor indica una mayor prioridad
        int Priority { get; }
        // Método para verificar si la regla se aplica al contexto actual
        bool IsMatch(DecisionContext context);
        // Método para crear una decisión basada en el contexto actual
        DecisionResult CreateDecision(DecisionContext context);
    }
}
