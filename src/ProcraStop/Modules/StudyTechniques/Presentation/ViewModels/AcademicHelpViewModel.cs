using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProcraStop.Application.StudyTechniques.DTOs;
using ProcraStop.Application.StudyTechniques.Services;
using ProcraStop.Domain.StudyTechniques.Enums;
using ProcraStop.Modules.StudyTechniques.Presentation.Models;

namespace ProcraStop.Modules.StudyTechniques.Presentation.ViewModels;

public partial class AcademicHelpViewModel : ObservableObject, IQueryAttributable
{
    private readonly AcademicHelpService _service;
    private readonly AcademicHelpMetricsService _metricsService;
    private Guid? _taskId;
    private Guid? _courseId;

    public AcademicHelpViewModel(
        AcademicHelpService service,
        AcademicHelpMetricsService metricsService)
    {
        _service = service;
        _metricsService = metricsService;
    }

    public ObservableCollection<HelpRequestItem> Requests { get; } = new();
    public IReadOnlyList<HelpChannelOption> Channels { get; } = HelpChannelOption.All;

    [ObservableProperty] private string topic = string.Empty;
    [ObservableProperty] private string question = string.Empty;
    [ObservableProperty] private HelpChannelOption selectedChannel = HelpChannelOption.All[0];
    [ObservableProperty] private string contactName = string.Empty;
    [ObservableProperty] private string feedback = string.Empty;
    [ObservableProperty] private bool isBusy;

    // D2 EPA: seguimiento visible de los tiempos de ayuda.
    [ObservableProperty] private bool hasMetrics;
    [ObservableProperty] private string metricsSummary = string.Empty;
    [ObservableProperty] private string requestTimeText = "Sin datos";
    [ObservableProperty] private string resolveTimeText = "Sin datos";
    [ObservableProperty] private string pendingText = "Sin consultas pendientes";
    [ObservableProperty] private string followUpText = string.Empty;
    [ObservableProperty] private bool hasPendingFollowUp;

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
        var now = DateTime.UtcNow;
        var requests = await _service.GetRecentAsync();
        Requests.Clear();
        foreach (var request in requests)
            Requests.Add(HelpRequestItem.FromDomain(request, now));

        ApplyMetrics(await _metricsService.GetAsync());
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

    private void ApplyMetrics(AcademicHelpMetricsDto metrics)
    {
        HasMetrics = metrics.HasMetrics;
        MetricsSummary =
            $"{metrics.Total} consulta(s) · {metrics.Planned} por consultar · " +
            $"{metrics.Requested} en curso · {metrics.Resolved} resueltas";
        RequestTimeText = FormatHours(metrics.AverageHoursToRequest);
        ResolveTimeText = FormatHours(metrics.AverageHoursToResolve);
        PendingText = metrics.OldestPendingDays.HasValue
            ? $"Pendientes: {metrics.Planned} · la más antigua hace {metrics.OldestPendingDays} día(s)"
            : $"Pendientes: {metrics.Planned}";
        HasPendingFollowUp = metrics.HasPendingFollowUp;
        FollowUpText = metrics.HasPendingFollowUp
            ? $"{metrics.PendingFollowUp} consulta(s) llevan más de " +
              $"{AcademicHelpMetricsDto.FollowUpThresholdDays} días sin realizarse."
            : string.Empty;
    }

    private static string FormatHours(double? hours)
    {
        if (!hours.HasValue)
            return "sin datos";
        if (hours.Value < 1)
            return "menos de 1 h";
        if (hours.Value < 48)
            return $"{hours.Value:0.#} h";

        return $"{hours.Value / 24:0.#} día(s)";
    }
}
