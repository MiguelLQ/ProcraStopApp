using ProcraStop.Modules.Academic.Presentation.ViewModels;
using ProcraStop.Shared.UI;

namespace ProcraStop.Modules.Academic.Presentation.Views;

public partial class CoursesPage : ContentPage
{
    private readonly CoursesPageViewModel _viewModel;

    public CoursesPage(CoursesPageViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await SafeUi.RunAsync(_viewModel.LoadAsync, "CoursesPage.OnAppearing");
    }
}
