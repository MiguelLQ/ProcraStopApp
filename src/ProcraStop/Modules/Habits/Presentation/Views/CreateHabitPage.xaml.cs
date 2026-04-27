using ProcraStop.Modules.Habits.Presentation.ViewModels;

namespace ProcraStop.Modules.Habits.Presentation.Views;

public partial class CreateHabitPage : ContentPage
{
	public CreateHabitPage(CreateHabitViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
    }
}