using ProcraStop.Modules.Tasks.Presentation.ViewModels;
using ProcraStop.Shared.Services;
using ProcraStop.Shared.UI.Layouts;
namespace ProcraStop.Modules.Tasks.Presentation.Views;

public partial class TaskDetailPage : BasePage
{
    public TaskDetailPage(TaskDetailViewModel vm, ISnackbarService snackbarService)
    {
        InitializeComponent();
        BindingContext = vm;

        InitializeSnackbar(snackbarService); //  CLAVE
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // recargar datos (lo que ya tenías)
        if (BindingContext is TaskDetailViewModel vm && vm.CurrentTaskId != Guid.Empty)
        {
            vm.SetTaskId(vm.CurrentTaskId);
        }

        // consumir snackbar pendiente
        var (msg, type) = PendingNotificationService.Consume();

        if (!string.IsNullOrWhiteSpace(msg))
        {
            await Task.Delay(300);

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await Snackbar.ShowAsync(msg, type);
            });
        }
    }
}
