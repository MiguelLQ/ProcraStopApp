using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProcraStop.Application.Habits.Ranking;
using ProcraStop.Modules.Habits.Presentation.Models;

namespace ProcraStop.Modules.Habits.Presentation.ViewModels;

public partial class HabitDisciplineRankingViewModel : ObservableObject
{
    private readonly HabitDisciplineRankingService _service;

    public HabitDisciplineRankingViewModel(HabitDisciplineRankingService service)
    {
        _service = service;
    }

    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private int activeHabits;
    [ObservableProperty] private int eliteHabits;
    [ObservableProperty] private int atRiskHabits;
    [ObservableProperty] private string averageScoreText = "0";
    [ObservableProperty] private string headlineText = "Sin datos suficientes";
    [ObservableProperty] private HabitDisciplineRankItem? firstPlace;
    [ObservableProperty] private HabitDisciplineRankItem? secondPlace;
    [ObservableProperty] private HabitDisciplineRankItem? thirdPlace;
    [ObservableProperty] private bool hasRanking;

    public ObservableCollection<HabitDisciplineRankItem> Ranking { get; } = new();

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            var data = await _service.GetAsync(CancellationToken.None);
            var items = data.Items.Select(item => new HabitDisciplineRankItem(item)).ToArray();

            ActiveHabits = data.ActiveHabits;
            EliteHabits = data.EliteHabits;
            AtRiskHabits = data.AtRiskHabits;
            AverageScoreText = $"{data.AverageScore:0}";
            HeadlineText = BuildHeadline(data.AverageScore, data.AtRiskHabits, data.ActiveHabits);
            FirstPlace = items.ElementAtOrDefault(0);
            SecondPlace = items.ElementAtOrDefault(1);
            ThirdPlace = items.ElementAtOrDefault(2);
            HasRanking = items.Length > 0;

            Ranking.Clear();
            foreach (var item in items)
                Ranking.Add(item);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static string BuildHeadline(double averageScore, int atRiskHabits, int activeHabits)
    {
        if (activeHabits == 0)
            return "Crea hábitos para medir tu disciplina!";

        if (atRiskHabits > 0)
            return "Hay hábitos que conviene rescatar, empieza ahora!";

        if (averageScore >= 85)
            return "Tu disciplina está en nivel alto, buen trabajo";

        if (averageScore >= 65)
            return "Tu sistema se mantiene estable, sigue así!!";

        return "La disciplina necesita consolidarse";
    }
}
