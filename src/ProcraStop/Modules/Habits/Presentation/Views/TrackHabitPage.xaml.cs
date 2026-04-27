using ProcraStop.Modules.Habits.Presentation.ViewModels;

namespace ProcraStop.Modules.Habits.Presentation.Views;

public partial class TrackHabitPage : ContentPage
{
    private readonly TrackHabitViewModel _vm;

    public TrackHabitPage(TrackHabitViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
        _vm = vm;
    }
  
}