

namespace ProcraStop.Core.Decisions.DTOs
{
    public enum DecisionCategory
    {
        Stress,
        Behavior,
        Performance,

        Risk,        // deterioro
        Recovery     // mejora desde estado negativo
    }
}
