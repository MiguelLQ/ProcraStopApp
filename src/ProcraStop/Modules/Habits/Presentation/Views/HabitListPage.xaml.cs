using ProcraStop.Modules.Habits.Presentation.ViewModels;

namespace ProcraStop.Modules.Habits.Presentation.Views;

public partial class HabitListPage : ContentPage
{
	public HabitListPage(HabitListViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is HabitListViewModel vm)
            await vm.Load();
    }
}