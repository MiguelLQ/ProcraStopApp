using ProcraStop.Modules.StudyTechniques.Presentation.ViewModels;
using ProcraStop.Shared.UI;

namespace ProcraStop.Modules.StudyTechniques.Presentation.Views;

public partial class AcademicHelpPage : ContentPage
{
    private readonly AcademicHelpViewModel _vm;

    public AcademicHelpPage(AcademicHelpViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await SafeUi.RunAsync(_vm.LoadAsync, "AcademicHelpPage.OnAppearing");
    }
}
