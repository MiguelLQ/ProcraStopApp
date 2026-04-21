using CommunityToolkit.Mvvm.ComponentModel;
using ProcraStop.Modules.Tasks.Application.EngineTask.Queries;
using ProcraStop.Modules.Tasks.Application.EngineTask.DTOs;
using System.Collections.ObjectModel;

namespace ProcraStop.Modules.Tasks.Presentation.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly GetDashboardDecisionHandler _decisionHandler;
        public DashboardViewModel(GetDashboardDecisionHandler decisionHandler)

        {
            _decisionHandler = decisionHandler;
        }
        // propiedad observable para el mensaje, tipo de decisión y acción sugerida
        [ObservableProperty]
        private string message = string.Empty;
        // el tipo de decisión puede ser una enumeración que indique si es una advertencia,
        // una recomendación o una acción crítica
        [ObservableProperty]
        private GuidanceType type;
        // la acción sugerida puede ser un texto que indique qué hacer a continuación,
        // o un enlace a una sección específica de la aplicación
        [ObservableProperty]
        private string? suggestedAction;
        // la razón detrás de la decisión puede ser un texto que explique por qué se tomó esa decisión,
        [ObservableProperty]
        private string reason = string.Empty;
        // las ideas o insights relacionados con la decisión pueden ser una lista de puntos clave que
        // el usuario debe tener en cuenta,
        [ObservableProperty]
        private ObservableCollection<string> insights = new();
        //
        [ObservableProperty]
        private Color? guidanceColor;
        // iconos
        [ObservableProperty]
        private string guidanceIcon = string.Empty;
        //
        [ObservableProperty]
        private ObservableCollection<string> secondaryInsights = new();
        public async Task LoadAsync()
        {
            // aquí se llama al handler para obtener la decisión del dashboard
            var decision = await _decisionHandler.Handle(new GetDashboardDecisionQuery());
            // luego se actualizan las propiedades observables con la información obtenida
            Message = decision.Message;
            Type = decision.Type;
            SuggestedAction = decision.SuggestedAction;
            Reason = decision.Reason ?? string.Empty;
            Insights = new ObservableCollection<string>(
                decision.Insights ?? new List<string>());
            GuidanceColor = MapColor(decision.Type);
            GuidanceIcon = MapIcon(decision.Type);
            SecondaryInsights = new ObservableCollection<string>(
                 decision.SecondaryInsights ?? new List<string>());
        }
        private Color MapColor(GuidanceType type)
        {
            return type switch
            {
                GuidanceType.Warning => Colors.OrangeRed,
                GuidanceType.Success => Colors.Green,
                GuidanceType.Info => Colors.Blue,
                _ => Colors.Gray
            };
        }
        private string MapIcon(GuidanceType type)
        {
            return type switch
            {
                GuidanceType.Warning => "⚠️",
                GuidanceType.Success => "✅",
                GuidanceType.Info => "💡",
                _ => "ℹ️"
            };
        }

    }
}
