using ProcraStop.Modules.Tasks.Application.Abstractions;
using ProcraStop.Modules.Tasks.Application.DTOs;
using ProcraStop.Modules.Tasks.Domain.Entities;
using ProcraStop.Modules.Tasks.Domain.Enums;

namespace ProcraStop.Modules.Tasks.Application.Queries.GetDashboardGuidance
{
    public class GetDashboardGuidanceHandler
    {
        private readonly ITaskRepository _repository;

        public GetDashboardGuidanceHandler(ITaskRepository repository)
        {
            _repository = repository;
        }

        public async Task<DashboardGuidanceDto> Handle(GetDashboardGuidanceQuery query)
        {
            var tasks = await _repository.GetAllAsync();

            var pending = tasks.Where(t => !t.IsCompleted && !t.IsArchived).ToList();

            if (!pending.Any())
            {
                return new DashboardGuidanceDto
                {
                    Message = "No tienes tareas pendientes. Buen trabajo.",
                    Type = GuidanceType.Success
                };
            }

            var overdueTasks = pending.Where(t => t.IsOverdue).ToList();

            var totalEffort = pending.Sum(t => t.EstimatedMinutes ?? 30);
            var heavyTasks = pending.Count(t => t.GetEffort() == TaskEffort.Heavy);
            var quickTasks = pending.Count(t => t.GetEffort() == TaskEffort.Quick);

            var completionRate = CalculateCompletionPressure(tasks);

            // 🔴 1. CRÍTICO: atraso + carga alta
            if (overdueTasks.Count >= 3 && totalEffort > 300)
            {
                return new DashboardGuidanceDto
                {
                    Message = "Estás acumulando retrasos y carga alta. Reduce el alcance hoy.",
                    Type = GuidanceType.Warning
                };
            }

            // 🔴 2. OVERLOAD (sobrecarga mental)
            if (heavyTasks >= 5)
            {
                return new DashboardGuidanceDto
                {
                    Message = "Tienes muchas tareas pesadas. Empieza por dividirlas o hacer progreso parcial.",
                    Type = GuidanceType.Warning
                };
            }

            // 🟡 3. PROCRASTINACIÓN DETECTADA
            if (overdueTasks.Any() && quickTasks >= 2)
            {
                return new DashboardGuidanceDto
                {
                    Message = "Tienes tareas rápidas atrasadas. Hazlas ahora para desbloquear progreso.",
                    Type = GuidanceType.Info
                };
            }

            // 🟡 4. BAJA ACTIVIDAD
            if (completionRate < 0.2)
            {
                return new DashboardGuidanceDto
                {
                    Message = "Has avanzado poco últimamente. Intenta completar 1 tarea corta hoy.",
                    Type = GuidanceType.Info
                };
            }

            // 🟢 5. ESTADO NORMAL
            return new DashboardGuidanceDto
            {
                Message = "Vas bien. Mantén el ritmo y prioriza lo importante.",
                Type = GuidanceType.Info
            };
        }
        private double CalculateCompletionPressure(IEnumerable<StudyTask> tasks)
        {
            var total = tasks.Count();
            if (total == 0) return 1;

            var completed = tasks.Count(t => t.IsCompleted);

            return (double)completed / total;
        }
    }
}
