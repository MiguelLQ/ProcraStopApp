using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProcraStop.Application.Academic.DTOs;
using ProcraStop.Application.Academic.Facade;
using ProcraStop.Application.StudyTechniques.Abstractions;
using ProcraStop.Application.StudyTechniques.Commands.RegisterStudySession;
using ProcraStop.Application.StudyTechniques.Services;
using ProcraStop.Domain.StudyTechniques.Enums;
using ProcraStop.Modules.StudyTechniques.Presentation.Models;
using ProcraStop.Modules.StudyTechniques.Presentation.Views;

namespace ProcraStop.Modules.StudyTechniques.Presentation.ViewModels;

public partial class StudyTechniquesViewModel : ObservableObject, IQueryAttributable
{
    private readonly StudyDashboardService _dashboardService;
    private readonly StudyRecommendationEngine _recommendationEngine;
    private readonly StudyTechniqueCatalogService _catalogService;
    private readonly IStudyTechniqueSessionRepository _sessionRepository;
    private readonly RegisterStudySessionHandler _registerHandler;
    private readonly ICoursesFacade _coursesFacade;
    private Guid? _pendingCourseId;

    public StudyTechniquesViewModel(
        StudyDashboardService dashboardService,
        StudyRecommendationEngine recommendationEngine,
        StudyTechniqueCatalogService catalogService,
        IStudyTechniqueSessionRepository sessionRepository,
        RegisterStudySessionHandler registerHandler,
        ICoursesFacade coursesFacade)
    {
        _dashboardService = dashboardService;
        _recommendationEngine = recommendationEngine;
        _catalogService = catalogService;
        _sessionRepository = sessionRepository;
        _registerHandler = registerHandler;
        _coursesFacade = coursesFacade;

        foreach (var technique in _catalogService.GetAll())
            Techniques.Add(new StudyTechniqueOption(technique));

        SelectedTechnique = Techniques.FirstOrDefault();
    }

    public ObservableCollection<StudyTechniqueOption> Techniques { get; } = new();
    public ObservableCollection<CourseDto> Courses { get; } = new();

    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private CourseDto? selectedCourse;
    [ObservableProperty] private string topic = "";
    [ObservableProperty] private int focus = 3;
    [ObservableProperty] private int energy = 3;
    [ObservableProperty] private int confidence = 3;
    [ObservableProperty] private int confusion = 3;
    [ObservableProperty] private int progressPercent = 50;
    [ObservableProperty] private int availableMinutes = 25;
    [ObservableProperty] private int completedPomodoros;
    [ObservableProperty] private string notes = "";
    [ObservableProperty] private StudyTechniqueOption? selectedTechnique;
    [ObservableProperty] private string recommendationTitle = "Recomendación lista";
    [ObservableProperty] private string recommendationReason = "Completa el estado del estudiante para ajustar la técnica.";
    [ObservableProperty] private string recommendationAction = "Empieza por una técnica corta y registra el resultado.";
    [ObservableProperty] private string helpNeedText = "0/100";
    [ObservableProperty] private string helpPrompt = "Aún no hay señales suficientes.";
    [ObservableProperty] private string sessionsThisWeekText = "0";
    [ObservableProperty] private string minutesThisWeekText = "0";
    [ObservableProperty] private string pomodorosThisWeekText = "0";
    [ObservableProperty] private string averageProgressText = "0%";
    [ObservableProperty] private string helpIndicatorText = "Sin datos";
    [ObservableProperty] private string mostUsedTechniqueText = "Sin datos";
    [ObservableProperty] private string saveMessage = "";

    public string FocusValueText => $"{Focus}/5";
    public string EnergyValueText => $"{Energy}/5";
    public string ConfidenceValueText => $"{Confidence}/5";
    public string ConfusionValueText => $"{Confusion}/5";
    public string ProgressValueText => $"{ProgressPercent}%";
    public string AvailableMinutesText => $"{AvailableMinutes} min";

    partial void OnFocusChanged(int value) => OnPropertyChanged(nameof(FocusValueText));
    partial void OnEnergyChanged(int value) => OnPropertyChanged(nameof(EnergyValueText));
    partial void OnConfidenceChanged(int value) => OnPropertyChanged(nameof(ConfidenceValueText));
    partial void OnConfusionChanged(int value) => OnPropertyChanged(nameof(ConfusionValueText));
    partial void OnProgressPercentChanged(int value) => OnPropertyChanged(nameof(ProgressValueText));
    partial void OnAvailableMinutesChanged(int value) => OnPropertyChanged(nameof(AvailableMinutesText));

    [RelayCommand]
    public async Task LoadAsync()
    {
        await LoadCoursesAsync();
        await LoadDashboardAsync();
        await RecommendAsync();
    }

    [RelayCommand]
    public async Task GoToPomodoroAsync()
    {
        await Shell.Current.GoToAsync(nameof(PomodoroPage));
    }

    [RelayCommand]
    public Task GoToHelpAsync()
    {
        var parameters = new Dictionary<string, object> { ["topic"] = Topic };
        if (SelectedCourse is not null)
            parameters["courseId"] = SelectedCourse.Id.ToString();
        return Shell.Current.GoToAsync(nameof(AcademicHelpPage), parameters);
    }

    [RelayCommand]
    public async Task RecommendAsync()
    {
        var recent = await _sessionRepository.GetRecentAsync(14, CancellationToken.None);
        var recommendation = _recommendationEngine.Recommend(
            Focus,
            Energy,
            Confidence,
            Confusion,
            AvailableMinutes,
            recent);

        RecommendationTitle = recommendation.Message;
        RecommendationReason = recommendation.Reason;
        RecommendationAction = recommendation.SuggestedAction;
        HelpNeedText = $"{recommendation.AcademicHelpNeedScore}/100";
        HelpPrompt = recommendation.HelpPrompt;
        SelectedTechnique = Techniques.FirstOrDefault(t => t.Type == recommendation.Technique) ?? SelectedTechnique;
        AvailableMinutes = recommendation.RecommendedMinutes;
    }

    [RelayCommand]
    public async Task SaveSessionAsync()
    {
        if (IsBusy)
            return;

        if (SelectedCourse is null)
        {
            SaveMessage = "Selecciona un curso registrado antes de guardar.";
            return;
        }

        try
        {
            IsBusy = true;
            var helpStatus = Confusion >= 4 && Confidence <= 2
                ? AcademicHelpStatus.Suggested
                : AcademicHelpStatus.NotNeeded;

            await _registerHandler.Handle(new RegisterStudySessionCommand
            {
                CourseName = SelectedCourse.Name,
                Topic = Topic,
                Technique = SelectedTechnique?.Type ?? StudyTechniqueType.Pomodoro,
                PlannedMinutes = AvailableMinutes,
                CompletedMinutes = AvailableMinutes,
                FocusBefore = Focus,
                EnergyBefore = Energy,
                ConfidenceBefore = Confidence,
                ConfusionAfter = Confusion,
                ProgressPercent = ProgressPercent,
                PomodoroCycles = CompletedPomodoros,
                HelpStatus = helpStatus,
                Notes = Notes
            }, CancellationToken.None);

            Notes = "";
            Topic = "";
            ProgressPercent = 50;
            CompletedPomodoros = 0;
            SaveMessage = "Sesión guardada. La próxima recomendación usará este historial.";
            await LoadDashboardAsync();
            await RecommendAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadCoursesAsync()
    {
        var courses = await _coursesFacade.GetCoursesAsync();
        Courses.Clear();

        foreach (var course in courses)
            Courses.Add(course);

        if (_pendingCourseId.HasValue)
        {
            SelectedCourse = Courses.FirstOrDefault(c => c.Id == _pendingCourseId.Value) ?? Courses.FirstOrDefault();
            _pendingCourseId = null;
        }
        else
        {
            SelectedCourse ??= Courses.FirstOrDefault();
        }
    }

    private async Task LoadDashboardAsync()
    {
        var dashboard = await _dashboardService.GetAsync(CancellationToken.None);
        SessionsThisWeekText = dashboard.SessionsThisWeek.ToString();
        MinutesThisWeekText = dashboard.MinutesThisWeek.ToString();
        PomodorosThisWeekText = dashboard.PomodorosThisWeek.ToString();
        AverageProgressText = $"{dashboard.AverageProgress}%";
        HelpIndicatorText = dashboard.HelpIndicatorText;
        MostUsedTechniqueText = dashboard.MostUsedTechnique;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("topic", out var topic) && topic is not null)
            Topic = topic.ToString() ?? "";

        if (query.TryGetValue("courseId", out var courseId) &&
            Guid.TryParse(courseId?.ToString(), out var parsedCourseId))
        {
            _pendingCourseId = parsedCourseId;
            SelectedCourse = Courses.FirstOrDefault(c => c.Id == parsedCourseId) ?? SelectedCourse;
        }

        if (query.TryGetValue("minutes", out var minutes) &&
            int.TryParse(minutes?.ToString(), out var parsedMinutes))
        {
            AvailableMinutes = Math.Clamp(parsedMinutes, 10, 60);
        }
    }
}
