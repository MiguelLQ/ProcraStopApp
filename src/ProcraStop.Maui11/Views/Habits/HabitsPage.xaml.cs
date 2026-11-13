using ProcraStop.Maui11.ViewModels.Habits;

namespace ProcraStop.Maui11.Views.Habits;

public partial class HabitsPage : ContentPage
{
    public HabitsPage(HabitsPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
