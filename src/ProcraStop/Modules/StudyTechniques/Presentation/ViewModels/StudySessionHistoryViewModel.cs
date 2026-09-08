using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProcraStop.Application.StudyTechniques.Abstractions;
using ProcraStop.Modules.StudyTechniques.Presentation.Models;

namespace ProcraStop.Modules.StudyTechniques.Presentation.ViewModels;

public partial class StudySessionHistoryViewModel : ObservableObject
{
    private readonly IStudyTechniqueSessionRepository _repository;

    public StudySessionHistoryViewModel(IStudyTechniqueSessionRepository repository)
    {
        _repository = repository;
    }

    public ObservableCollection<StudySessionHistoryItem> Sessions { get; } = new();

    [ObservableProperty] private bool isBusy;

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            var sessions = await _repository.GetRecentAsync(30, CancellationToken.None);
            Sessions.Clear();
            foreach (var session in sessions)
                Sessions.Add(new StudySessionHistoryItem(session));
        }
        finally
        {
            IsBusy = false;
        }
    }
}
