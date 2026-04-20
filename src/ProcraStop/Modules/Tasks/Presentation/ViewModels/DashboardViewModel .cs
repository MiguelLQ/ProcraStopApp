using CommunityToolkit.Mvvm.ComponentModel;
using ProcraStop.Modules.Tasks.Application.DecisionEngine.Queries;
using ProcraStop.Modules.Tasks.Domain.Enums;

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
        // método para cargar la decisión del dashboard
        public async Task LoadAsync()
        {
            // aquí se llama al handler para obtener la decisión del dashboard
            var decision = await _decisionHandler.Handle(new GetDashboardDecisionQuery());
            // luego se actualizan las propiedades observables con la información obtenida
            Message = decision.Message;
            Type = decision.Type;
            SuggestedAction = decision.SuggestedAction;
        }

    }
}
