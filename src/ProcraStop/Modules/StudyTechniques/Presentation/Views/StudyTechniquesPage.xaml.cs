using ProcraStop.Modules.StudyTechniques.Presentation.ViewModels;

namespace ProcraStop.Modules.StudyTechniques.Presentation.Views;

public partial class StudyTechniquesPage : ContentPage
{
    private readonly StudyTechniquesHomeViewModel _vm;

    public StudyTechniquesPage(StudyTechniquesHomeViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadAsync();
    }
}
