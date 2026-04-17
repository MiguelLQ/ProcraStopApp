using ProcraStop.Modules.Tasks.Presentation.ViewModels;
using ProcraStop.Shared.Helpers;
using ProcraStop.Shared.Services;
using ProcraStop.Shared.UI.Components;
namespace ProcraStop.Modules.Tasks.Presentation.Views;

public partial class TaskDetailPage : ContentPage
{
    private readonly ISnackbarService _snackbarService;

    public TaskDetailPage(TaskDetailViewModel vm, ISnackbarService snackbarService)
    {
        InitializeComponent();
        BindingContext = vm;
        _snackbarService = snackbarService;

    }
    private SnackbarService? _concrete;
    private async void OnPageTapped(object sender, EventArgs e)
    {
        if (SnackbarView.IsVisible)
        {
            await SnackbarView.Hide();
        }
    }
    private async Task HandleSnackbar(string msg, SnackbarType type)
    {
        var (icon, color) = type.GetStyle();

        Overlay.IsVisible = true;

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await SnackbarView.Show(msg, icon, color);
        });

        // Se ejecuta DESPUÉS del click
        Overlay.IsVisible = false;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();

        _concrete = _snackbarService as SnackbarService;

        if (_concrete is not null)
        {
            _concrete.OnShowRequested += HandleSnackbar;
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        if (_concrete is not null)
        {
            _concrete.OnShowRequested -= HandleSnackbar;
        }
    }
    private async void OnOverlayTapped(object sender, EventArgs e)
    {
        await SnackbarView.Hide();
        Overlay.IsVisible = false;
    }
}
