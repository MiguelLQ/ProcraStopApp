
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ProcraStop.Application.Dashboard.Models;
using ProcraStop.Application.Dashboard.Models.Historial;
using ProcraStop.Application.Dashboard.Queries;
using ProcraStop.Application.Dashboard.Interfaces;
using ProcraStop.Modules.Dashboard.Presentation.Models;
using ProcraStop.Modules.Stress.Presentation.Messages;
using ProcraStop.Core.Decisions.DTOs;
using System.Collections.ObjectModel;
namespace ProcraStop.Modules.Dashboard.Presentation.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly GetDashboardDecisionHandler _handler;
        private readonly IDashboardMaintenanceService _maintenance;

        public DashboardViewModel(
            GetDashboardDecisionHandler handler,
            IDashboardMaintenanceService maintenance)
        {
            _handler = handler;
            _maintenance = maintenance;
            WeakReferenceMessenger.Default.Register<StressCheckInSubmittedMessage>(
                this,
                async (_, _) => await LoadAsync());
        }

        // =========================
        // CORE STATE
        // =========================

        [ObservableProperty]
        private string message = string.Empty;

        [ObservableProperty]
        private string reason = string.Empty;

        [ObservableProperty]
        private string suggestedAction = string.Empty;

        [ObservableProperty]
        private string stateColor = "#6B7280";

        [ObservableProperty]
        private string stateTone = "Estado del sistema";

        [ObservableProperty]
        private string healthScoreText = "0%";

        [ObservableProperty]
        private string systemLabel = "Sin datos";

        [ObservableProperty]
        private string trendSummary = "Tendencia estable";

        [ObservableProperty]
        private double[] balanceValues = Array.Empty<double>();

        [ObservableProperty]
        private string[] balanceLabels = Array.Empty<string>();

        // =========================
        // SYSTEM STATE
        // =========================

        [ObservableProperty]
        private SystemStateReadModel? systemState;

        // =========================
        // COLLECTIONS
        // =========================

        public ObservableCollection<SignalReadModel> Signals { get; } = new();

        public ObservableCollection<string> TaskInsights { get; } = new();
        public ObservableCollection<string> HabitInsights { get; } = new();
        public ObservableCollection<string> StressInsights { get; } = new();
        public ObservableCollection<DashboardInsightItem> InsightCards { get; } = new();
        public ObservableCollection<DashboardSignalGroup> SignalGroups { get; } = new();

        public ObservableCollection<CategoryBalanceReadModel> Balance { get; } = new();
        public ObservableCollection<TrendReadModel> Trends { get; } = new();
        //
        [ObservableProperty]
        private TrendReadModel? currentTrend;
        // =========================
        // LOAD
        // =========================

        [RelayCommand]
        public async Task LoadAsync()
        {
            var data = await _handler.Handle(new GetDashboardDecisionQuery());

            if (data is null) return;

            // CORE
            Message = data.CoreState?.Message ?? "";
            Reason = data.CoreState?.Reason ?? "";
            SuggestedAction = data.CoreState?.SuggestedAction ?? "";
            StateColor = ResolveColor(data.CoreState?.Type);
            StateTone = ResolveTone(data.CoreState?.Type);

            // SYSTEM STATE
            SystemState = data.SystemState;
            HealthScoreText = data.SystemState is null ? "0%" : $"{data.SystemState.OverallScore:P0}";
            SystemLabel = TranslateSystemLabel(data.SystemState?.Label);

            // SIGNALS
            Signals.Clear();
            foreach (var s in data.Signals ?? new List<SignalReadModel>())
                Signals.Add(s);
            BuildSignalGroups(data.Signals);

            // INSIGHTS
            Fill(TaskInsights, data.TaskInsights);
            Fill(HabitInsights, data.HabitInsights);
            Fill(StressInsights, data.StressInsights);
            BuildInsightCards(data);

            // BALANCE
            Balance.Clear();
            foreach (var b in data.CategoryBalance ?? new List<CategoryBalanceReadModel>())
                Balance.Add(b);
            BuildBalance(data.CategoryBalance?.FirstOrDefault());

            // TREND ✅ FIX REAL
            CurrentTrend = data.Trend;
            TrendSummary = ResolveTrendSummary(data.Trend);

            _ = RecordDashboardStateSafelyAsync(data);
        }

        private async Task RecordDashboardStateSafelyAsync(DashboardReadModel data)
        {
            try
            {
                await _maintenance.RecordLatestStateAsync(data);
            }
            catch (Exception exception)
            {
                Console.WriteLine($"[DashboardMaintenance] {exception}");
            }
        }

        // =========================
        // HELPERS
        // =========================

        private void Fill(ObservableCollection<string> target, List<string>? source)
        {
            target.Clear();

            if (source == null) return;

            foreach (var s in source)
                target.Add(s);
        }

        private string ResolveColor(DecisionType? type)
        {
            return type switch
            {
                DecisionType.Critical => "#FF5A5F",
                DecisionType.Warning => "#FFB020",
                DecisionType.Success => "#22C55E",
                _ => "#6B7280"
            };
        }

        private static string ResolveTone(DecisionType? type)
        {
            return type switch
            {
                DecisionType.Critical => "Riesgo detectado",
                DecisionType.Warning => "Atención recomendada",
                DecisionType.Success => "Ritmo saludable",
                _ => "Lectura del sistema"
            };
        }

        private static string TranslateSystemLabel(string? label)
        {
            return label switch
            {
                "Overloaded" => "Sobrecargado",
                "Optimal" => "Óptimo",
                "Stable" => "Estable",
                "At Risk" => "En riesgo",
                _ => "Sin datos"
            };
        }

        private static string ResolveTrendSummary(TrendReadModel? trend)
        {
            return trend?.GlobalTrend switch
            {
                "Improving" => "El sistema está mejorando",
                "Degrading" => "La carga está empeorando",
                "Insufficient Data" => "Recolectando historial",
                _ => "Tendencia estable"
            };
        }

        private void BuildBalance(CategoryBalanceReadModel? balance)
        {
            if (balance is null)
            {
                BalanceValues = Array.Empty<double>();
                BalanceLabels = Array.Empty<string>();
                return;
            }

            BalanceValues = new[]
            {
                balance.Performance,
                balance.Behavior,
                balance.Stress
            };
            BalanceLabels = new[] { "Tareas", "Hábitos", "Stress" };
        }

        private void BuildInsightCards(DashboardReadModel data)
        {
            InsightCards.Clear();
            AddInsights("Tareas", data.TaskInsights, "#4F8CFF");
            AddInsights("Hábitos", data.HabitInsights, "#34C77B");
            AddInsights("Stress", data.StressInsights, "#EF5B5B");
        }

        private void AddInsights(string source, List<string>? insights, string color)
        {
            if (insights is null)
                return;

            foreach (var insight in insights.Where(x => !string.IsNullOrWhiteSpace(x)).Take(3))
            {
                InsightCards.Add(new DashboardInsightItem
                {
                    Source = source,
                    Initial = source[..1],
                    Text = insight,
                    AccentColor = color
                });
            }
        }

        private void BuildSignalGroups(List<SignalReadModel>? signals)
        {
            SignalGroups.Clear();
            if (signals is null)
                return;

            foreach (var group in signals
                .GroupBy(x => x.Category)
                .OrderByDescending(x => x.Max(s => s.Score))
                .Take(4))
            {
                SignalGroups.Add(new DashboardSignalGroup
                {
                    Category = TranslateCategory(group.Key),
                    AccentColor = ResolveCategoryColor(group.Key),
                    Signals = group.OrderByDescending(x => x.Score).Take(3).ToList()
                });
            }
        }

        private static string TranslateCategory(string category)
        {
            return category switch
            {
                "Performance" => "Rendimiento",
                "Behavior" => "Comportamiento",
                "Stress" => "Stress",
                "Risk" => "Riesgo",
                "Trajectory" => "Trayectoria",
                "Recovery" => "Recuperación",
                "Capacity" => "Capacidad",
                _ => "Sistema"
            };
        }

        private static string ResolveCategoryColor(string category)
        {
            return category switch
            {
                "Performance" => "#4F8CFF",
                "Behavior" => "#34C77B",
                "Stress" => "#EF5B5B",
                "Risk" => "#FFB020",
                "Trajectory" => "#8B5CF6",
                "Recovery" => "#14B8A6",
                "Capacity" => "#F59E0B",
                _ => "#91A0B6"
            };
        }
    }
}
