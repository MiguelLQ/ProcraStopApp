using ProcraStop.Maui11.ViewModels.Settings;

namespace ProcraStop.Maui11.Views.Settings;

public partial class SettingsPage : ContentPage
{
    public SettingsPage(SettingsPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
