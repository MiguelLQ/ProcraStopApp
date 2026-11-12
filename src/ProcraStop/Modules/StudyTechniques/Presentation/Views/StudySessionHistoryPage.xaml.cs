using ProcraStop.Modules.StudyTechniques.Presentation.ViewModels;
using ProcraStop.Shared.UI;

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
        await SafeUi.RunAsync(_vm.LoadAsync, "StudySessionHistoryPage.OnAppearing");
    }
}
