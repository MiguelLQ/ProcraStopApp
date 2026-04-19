using ProcraStop.Modules.Tasks.Presentation.ViewModels;
namespace ProcraStop.Modules.Tasks.Presentation.Views;

public partial class DashboardPage : ContentPage
{
    public DashboardPage(DashboardViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is DashboardViewModel vm)
        {
            await vm.LoadAsync();
        }
    }
}