using ProcraStop.Modules.StudyTechniques.Presentation.ViewModels;

namespace ProcraStop.Modules.StudyTechniques.Presentation.Views;

public partial class StudySessionHistoryPage : ContentPage
{
    private readonly StudySessionHistoryViewModel _vm;

    public StudySessionHistoryPage(StudySessionHistoryViewModel vm)
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
