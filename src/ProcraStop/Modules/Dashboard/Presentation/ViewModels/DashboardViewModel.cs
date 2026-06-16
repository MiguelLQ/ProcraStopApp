
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProcraStop.Application.Dashboard.Models;
using ProcraStop.Application.Dashboard.Models.Historial;
using ProcraStop.Application.Dashboard.Queries;
using ProcraStop.Core.Decisions.DTOs;
using System.Collections.ObjectModel;
namespace ProcraStop.Modules.Dashboard.Presentation.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly GetDashboardDecisionHandler _handler;

        public DashboardViewModel(GetDashboardDecisionHandler handler)
        {
            _handler = handler;
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

            // SYSTEM STATE
            SystemState = data.SystemState;

            // SIGNALS
            Signals.Clear();
            foreach (var s in data.Signals ?? new List<SignalReadModel>())
                Signals.Add(s);

            // INSIGHTS
            Fill(TaskInsights, data.TaskInsights);
            Fill(HabitInsights, data.HabitInsights);
            Fill(StressInsights, data.StressInsights);

            // BALANCE
            Balance.Clear();
            foreach (var b in data.CategoryBalance ?? new List<CategoryBalanceReadModel>())
                Balance.Add(b);

            // TREND ✅ FIX REAL
            CurrentTrend = data.Trend;
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
    }
}
