using ProcraStop.Maui11.ViewModels.DesignSystem;

namespace ProcraStop.Maui11.Views.DesignSystem;

public partial class DesignSystemPage : ContentPage
{
    public DesignSystemPage(DesignSystemViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
