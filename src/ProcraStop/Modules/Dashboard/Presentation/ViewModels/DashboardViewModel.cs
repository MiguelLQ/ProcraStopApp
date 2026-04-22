
using CommunityToolkit.Mvvm.ComponentModel;
using ProcraStop.Modules.Dashboard.Application.Queries;
namespace ProcraStop.Modules.Dashboard.Presentation.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly GetDashboardDecisionHandler _handler;

        [ObservableProperty]
        private string title = string.Empty;

        [ObservableProperty]
        private string message = string.Empty;

        [ObservableProperty]
        private string? reason;

        [ObservableProperty]
        private List<string> insights = new();

        [ObservableProperty]
        private string? suggestedAction;

        [ObservableProperty]
        private string typeColor = string.Empty; // opcional UX

        public DashboardViewModel(GetDashboardDecisionHandler handler)
        {
            _handler = handler;
        }

        public async Task LoadAsync()
        {
            var result = await _handler.Handle(new GetDashboardDecisionQuery());

            Message = result.Message;
            Reason = result.Reason;
            Insights = result.Insights ?? new();
            SuggestedAction = result.SuggestedAction;

            //  SOLO UI INTERPRETATION (NO lógica de dominio)
            TypeColor = MapColor(result.Type);
        }

        private static string MapColor(ProcraStop.Core.Decisions.DTOs.DecisionType type)
        {
            return type switch
            {
                ProcraStop.Core.Decisions.DTOs.DecisionType.Critical => "Red",
                ProcraStop.Core.Decisions.DTOs.DecisionType.Warning => "Orange",
                ProcraStop.Core.Decisions.DTOs.DecisionType.Success => "Green",
                _ => "Blue"
            };
        }
    }
}
