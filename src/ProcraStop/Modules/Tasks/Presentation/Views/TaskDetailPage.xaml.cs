using ProcraStop.Modules.Tasks.Presentation.ViewModels;
using ProcraStop.Shared.Services;
using ProcraStop.Shared.UI;
using ProcraStop.Shared.UI.Layouts;

namespace ProcraStop.Modules.Tasks.Presentation.Views;

public partial class TaskDetailPage : BasePage
{
    public TaskDetailPage(TaskDetailViewModel vm, ISnackbarService snackbarService)
    {
        InitializeComponent();
        BindingContext = vm;

        InitializeSnackbar(snackbarService, SnackbarHost); // CLAVE
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await SafeUi.RunAsync(async () =>
        {
            // Recargar datos de la tarea actual.
            if (BindingContext is TaskDetailViewModel vm && vm.CurrentTaskId != Guid.Empty)
            {
                vm.SetTaskId(vm.CurrentTaskId);
            }

            // Consumir snackbar pendiente.
            var (msg, type) = PendingNotificationService.Consume();

            if (!string.IsNullOrWhiteSpace(msg))
            {
                await Task.Delay(300);

                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await ShowSnackbarAsync(msg, type);
                });
            }
        }, "TaskDetailPage.OnAppearing");
    }
}
