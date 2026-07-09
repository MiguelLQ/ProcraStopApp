using ProcraStop.Modules.Habits.Presentation.ViewModels;
using CommunityToolkit.Maui.Core;
using ProcraStop.Shared.Services;
using ProcraStop.Shared.UI.Layouts;

namespace ProcraStop.Modules.Habits.Presentation.Views;

public partial class HabitListPage : BasePage
{
    // editbycodex: hereda color primario y contenido claro desde BasePage.

	public HabitListPage(HabitListViewModel vm, ISnackbarService snackbarService)
	{
		InitializeComponent();
        BindingContext = vm;
        InitializeSnackbar(snackbarService); // editbycodex: feedback consistente con Tasks.
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is HabitListViewModel vm)
            await vm.Load();

        var (message, type) = PendingNotificationService.Consume();
        if (!string.IsNullOrWhiteSpace(message))
            await Snackbar.ShowAsync(message, type); // editbycodex: confirma crear, editar o pausar al volver a Hoy.
    }
}
