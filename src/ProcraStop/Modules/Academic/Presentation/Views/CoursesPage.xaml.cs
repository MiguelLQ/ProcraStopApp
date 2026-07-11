using ProcraStop.Modules.Academic.Presentation.ViewModels;

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

        await _viewModel.LoadAsync();
    }
}
