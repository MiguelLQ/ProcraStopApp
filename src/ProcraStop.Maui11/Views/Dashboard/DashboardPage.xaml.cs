using ProcraStop.Maui11.ViewModels.Dashboard;

namespace ProcraStop.Maui11.Views.Dashboard;

public partial class DashboardPage : ContentPage
{
    public DashboardPage(DashboardViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
