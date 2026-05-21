
using ProcraStop.Application.Dashboard.Interfaces;
using ProcraStop.Application.Dashboard.Models;
using ProcraStop.Core.Decisions.Context;
using ProcraStop.Core.Decisions.DTOs;
using ProcraStop.Core.Decisions.Mappers;
using ProcraStop.Core.Decisions.Signals;

namespace ProcraStop.Application.Dashboard.Services
{
    public class SignalProjectionService : ISignalProjectionService
    {
        public List<SignalReadModel> Build(DecisionContext context, DecisionResult decision)
        {
            var signals = new List<UnifiedSignal>();

            // 1. TASKS → STATE BASED EXTRACTION
            signals.AddRange(TaskSignalExtractor.Extract(context.Tasks));

            // 2. HABITS → STATE BASED EXTRACTION
            signals.AddRange(HabitSignalExtractor.Extract(context.Habits));

            // 3. STRESS → STATE BASED EXTRACTION
            signals.AddRange(StressSignalExtractor.Extract(context.Stress));

            // 4. NORMALIZATION + ENRICHMENT
            return signals
                .GroupBy(s => NormalizeCategory(s.Category))
                .SelectMany(g => g.Select(s => new SignalReadModel
                {
                    Name = s.Name,
                    Category = g.Key,
                    Score = NormalizeScore(s.Score),
                    Type = s.Type
                }))
                .OrderByDescending(x => x.Score)
                .ToList();
        }

        // =========================
        // NORMALIZACIÓN UI
        // =========================

        private string NormalizeCategory(string category)
        {
            return category switch
            {
                "Performance" => "Performance",
                "Behavior" => "Behavior",
                "Stress" => "Stress",
                _ => "System"
            };
        }

        private double NormalizeScore(double score)
        {
            return Math.Clamp(Math.Round(score, 2), 0, 1);
        }
    }
}
