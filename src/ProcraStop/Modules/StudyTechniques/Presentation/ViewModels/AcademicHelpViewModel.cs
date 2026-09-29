using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProcraStop.Application.StudyTechniques.Services;
using ProcraStop.Domain.StudyTechniques.Enums;
using ProcraStop.Modules.StudyTechniques.Presentation.Models;

namespace ProcraStop.Modules.StudyTechniques.Presentation.ViewModels;

public partial class AcademicHelpViewModel : ObservableObject, IQueryAttributable
{
    private readonly AcademicHelpService _service;
    private Guid? _taskId;
    private Guid? _courseId;

    public AcademicHelpViewModel(AcademicHelpService service) => _service = service;

    public ObservableCollection<HelpRequestItem> Requests { get; } = new();
    public IReadOnlyList<HelpChannelOption> Channels { get; } = HelpChannelOption.All;

    [ObservableProperty] private string topic = string.Empty;
    [ObservableProperty] private string question = string.Empty;
    [ObservableProperty] private HelpChannelOption selectedChannel = HelpChannelOption.All[0];
    [ObservableProperty] private string contactName = string.Empty;
    [ObservableProperty] private string feedback = string.Empty;
    [ObservableProperty] private bool isBusy;

    public bool NeedsContactName => SelectedChannel.Value != HelpChannel.CourseMaterial;

    partial void OnSelectedChannelChanged(HelpChannelOption value)
        => OnPropertyChanged(nameof(NeedsContactName));

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        _taskId = null;
        _courseId = null;
        if (query.TryGetValue("topic", out var topicValue))
            Topic = topicValue?.ToString() ?? string.Empty;
        if (query.TryGetValue("taskId", out var taskValue) && Guid.TryParse(taskValue?.ToString(), out var taskId))
            _taskId = taskId;
        if (query.TryGetValue("courseId", out var courseValue) && Guid.TryParse(courseValue?.ToString(), out var courseId))
            _courseId = courseId;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        var requests = await _service.GetRecentAsync();
        Requests.Clear();
        foreach (var request in requests)
            Requests.Add(HelpRequestItem.FromDomain(request));
    }

    [RelayCommand]
    private async Task PlanAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            Feedback = string.Empty;
            await _service.PlanAsync(
                _taskId, _courseId, Topic, Question, SelectedChannel.Value,
                NeedsContactName ? ContactName : null);
            Question = string.Empty;
            ContactName = string.Empty;
            Feedback = "Consulta preparada. Cuando uses el recurso o hables con la persona, registra que la realizaste.";
            await LoadAsync();
        }
        catch (ArgumentException ex) { Feedback = ex.Message; }
        catch (InvalidOperationException ex) { Feedback = ex.Message; }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task MarkRequestedAsync(Guid id)
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            await _service.MarkRequestedAsync(id);
            Feedback = "Consulta realizada registrada.";
            await LoadAsync();
        }
        catch (InvalidOperationException ex) { Feedback = ex.Message; }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task ResolveAsync(Guid id)
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            await _service.MarkResolvedAsync(id);
            Feedback = "Duda marcada como resuelta.";
            await LoadAsync();
        }
        catch (InvalidOperationException ex) { Feedback = ex.Message; }
        finally { IsBusy = false; }
    }
}
