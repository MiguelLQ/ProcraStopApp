using ProcraStop.Modules.Habits.Presentation.ViewModels;

namespace ProcraStop.Modules.Habits.Presentation.Views;

public partial class HabitDisciplineRankingPage : ContentPage
{
    private readonly HabitDisciplineRankingViewModel _vm;

    public HabitDisciplineRankingPage(HabitDisciplineRankingViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadAsync();
    }
}
