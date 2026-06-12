
namespace ProcraStop.Application.Tasks.EngineTask.Signals
{
    public class TaskSignals
    {
        // Señales básicas (valores numéricos)
        public int PendingCount { get; init; }
        // Podríamos agregar más señales básicas como:
        public int OverdueCount { get; init; }
        // Por ejemplo, podríamos tener señales que indiquen la cantidad de tareas rápidas vs. tareas pesadas:
        public int QuickTasksCount { get; init; }
        // Y también podríamos tener una señal que indique el esfuerzo total estimado para completar las tareas pendientes:
        public int HeavyTasksCount { get; init; }
        // Finalmente, podríamos tener una señal que indique el esfuerzo total estimado para completar todas las tareas pendientes:
        public int TotalEffort { get; init; }
        // ============================================
        // Señales derivadas (booleanas)
        // ============================================
        // A partir de las señales básicas, podemos derivar señales booleanas que indiquen situaciones específicas:
        public bool HasOverdueTasks => OverdueCount > 0;
        // Por ejemplo, podríamos tener una señal que indique si el usuario tiene muchas tareas atrasadas:
        public bool HighOverdue => OverdueCount >= 3;
        // También podríamos tener una señal que indique si el usuario tiene muchas tareas rápidas pendientes:
        public bool HasQuickTasks => QuickTasksCount >= 2;
        // Y podríamos tener una señal que indique si el usuario tiene muchas tareas pesadas pendientes:
        public bool TooManyHeavyTasks => HeavyTasksCount >= 5;
        // Finalmente, podríamos tener una señal que indique si el esfuerzo total estimado para completar las tareas pendientes es alto:
        public bool HighTotalEffort => TotalEffort > 300;
        // Y por supuesto, podríamos tener una señal que indique si el usuario no tiene tareas pendientes:
        public bool NoPendingTasks => PendingCount == 0;
    
    }
}
