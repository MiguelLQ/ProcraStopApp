using ProcraStop.Modules.Dashboard.Presentation.ViewModels;

namespace ProcraStop.Modules.Dashboard.Presentation.Views;

public partial class DashboardView : ContentPage
{
    private readonly DashboardViewModel _vm;

    public DashboardView(DashboardViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadAsync();
        this.Opacity = 0;
        this.Scale = 0.98;

        await this.FadeToAsync(1, 250, Easing.CubicOut);
        await this.ScaleToAsync(1, 200, Easing.CubicOut);
    }
}