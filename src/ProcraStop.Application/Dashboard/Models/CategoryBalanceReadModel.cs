
using ProcraStop.Core.Decisions.DTOs;

namespace ProcraStop.Application.Dashboard.Models
{
    public class CategoryBalanceReadModel
    {
        public double Performance { get; set; }   // 0–1
        public double Behavior { get; set; }      // 0–1
        public double Stress { get; set; }        // 0–1

        public DecisionCategory DominantCategory { get; set; }

        public double ImbalanceScore { get; set; } // 0–1 (qué tan desbalanceado está)
    }
}
