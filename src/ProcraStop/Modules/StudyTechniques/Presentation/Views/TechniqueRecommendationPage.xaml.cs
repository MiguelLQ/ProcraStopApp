using ProcraStop.Modules.StudyTechniques.Presentation.ViewModels;
using ProcraStop.Shared.UI;

namespace ProcraStop.Modules.StudyTechniques.Presentation.Views;

public partial class TechniqueRecommendationPage : ContentPage
{
    private readonly StudyTechniquesViewModel _vm;

    public TechniqueRecommendationPage(StudyTechniquesViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await SafeUi.RunAsync(_vm.LoadAsync, "TechniqueRecommendationPage.OnAppearing");
    }
}
