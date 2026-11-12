using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProcraStop.Application.Academic.DTOs;
using ProcraStop.Application.Academic.Facade;
using ProcraStop.Application.StudyTechniques.Abstractions;
using ProcraStop.Application.StudyTechniques.Commands.RegisterStudySession;
using ProcraStop.Application.StudyTechniques.DTOs;
using ProcraStop.Application.StudyTechniques.Services;
using ProcraStop.Application.Tasks.Abstractions;
using ProcraStop.Application.Tasks.DTOs;
using ProcraStop.Domain.StudyTechniques.Enums;
using ProcraStop.Domain.Tasks.Enums;
using ProcraStop.Modules.StudyTechniques.Presentation.Models;
using ProcraStop.Modules.StudyTechniques.Presentation.Views;
using ProcraStop.Shared.Services;
using ProcraStop.Shared.UI;

namespace ProcraStop.Modules.StudyTechniques.Presentation.ViewModels;

public partial class StudyTechniquesViewModel : ObservableObject, IQueryAttributable
{
    private readonly StudyDashboardService _dashboardService;
    private readonly StudyRecommendationEngine _recommendationEngine;
    private readonly StudyTechniqueCatalogService _catalogService;
    private readonly IStudyTechniqueSessionRepository _sessionRepository;
    private readonly RegisterStudySessionHandler _registerHandler;
    private readonly ICoursesFacade _coursesFacade;
    private readonly ITaskReadService _taskReadService;
    private readonly ExamPreparationService _examPreparationService;
    private readonly ExamSimulationService _examSimulationService;
    private Guid? _pendingCourseId;
    private bool _loadingExams;

    public StudyTechniquesViewModel(
        StudyDashboardService dashboardService,
        StudyRecommendationEngine recommendationEngine,
        StudyTechniqueCatalogService catalogService,
        IStudyTechniqueSessionRepository sessionRepository,
        RegisterStudySessionHandler registerHandler,
        ICoursesFacade coursesFacade,
        ITaskReadService taskReadService,
        ExamPreparationService examPreparationService,
        ExamSimulationService examSimulationService)
    {
        _dashboardService = dashboardService;
        _recommendationEngine = recommendationEngine;
        _catalogService = catalogService;
        _sessionRepository = sessionRepository;
        _registerHandler = registerHandler;
        _coursesFacade = coursesFacade;
        _taskReadService = taskReadService;
        _examPreparationService = examPreparationService;
        _examSimulationService = examSimulationService;

        foreach (var technique in _catalogService.GetAll())
            Techniques.Add(new StudyTechniqueOption(technique));

        SelectedTechnique = Techniques.FirstOrDefault();
    }

    public ObservableCollection<StudyTechniqueOption> Techniques { get; } = new();
    public ObservableCollection<CourseDto> Courses { get; } = new();
    // D2 EPA: examenes del curso seleccionado para vincular la sesion.
    public ObservableCollection<TaskListItemDto> Exams { get; } = new();
    public ObservableCollection<string> ExamTopics { get; } = new();

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

    [ObservableProperty] private TaskListItemDto? selectedExam;
    [ObservableProperty] private string? selectedExamTopic;
    [ObservableProperty] private bool hasExamCoverage;
    [ObservableProperty] private string examCoverageText = "";
    [ObservableProperty] private string examCoverageDetail = "";

    // D2 EPA: simulacros del examen seleccionado.
    [ObservableProperty] private string simulationScoreText = "";
    [ObservableProperty] private string simulationErrors = "";
    [ObservableProperty] private string simulationMessage = "";
    [ObservableProperty] private string examSimulationText = "Sin simulacros registrados";
    [ObservableProperty] private bool hasExamSimulation;

    public bool HasExams => Exams.Count > 0;
    public bool HasSelectedExam => SelectedExam is not null;
    public bool HasExamTopics => ExamTopics.Count > 0;

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

    partial void OnSelectedCourseChanged(CourseDto? value)
    {
        if (!_loadingExams)
            _ = SafeUi.RunAsync(() => LoadExamsAsync(value), "StudyTechniquesViewModel.LoadExams");
    }

    partial void OnSelectedExamChanged(TaskListItemDto? value)
    {
        OnPropertyChanged(nameof(HasSelectedExam));
        _ = SafeUi.RunAsync(() => LoadExamTopicsAsync(value), "StudyTechniquesViewModel.LoadExamTopics");
    }

    partial void OnSelectedExamTopicChanged(string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            Topic = value;
    }

    [RelayCommand]
    private void ClearExam()
    {
        SelectedExam = null;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        await LoadCoursesAsync();
        await LoadExamsAsync(SelectedCourse);
        await LoadDashboardAsync();
        await RecommendAsync();
    }

    [RelayCommand]
    public async Task GoToPomodoroAsync()
    {
        // Pomodoro es un tab de Shell, no una ruta global.
        await ShellNavigation.GoToAsync("//PomodoroPage");
    }

    [RelayCommand]
    public Task GoToHelpAsync()
    {
        var parameters = new Dictionary<string, object> { ["topic"] = Topic };
        if (SelectedCourse is not null)
            parameters["courseId"] = SelectedCourse.Id.ToString();
        return ShellNavigation.GoToAsync(nameof(AcademicHelpPage), parameters);
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
                TaskId = SelectedExam?.Id,
                CourseName = SelectedCourse.Name,
                Topic = string.IsNullOrWhiteSpace(SelectedExamTopic) ? Topic : SelectedExamTopic,
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
            if (SelectedExam is not null)
                await RefreshExamCoverageAsync(SelectedExam.Id);
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

    private async Task LoadExamsAsync(CourseDto? course)
    {
        if (_loadingExams)
            return;

        _loadingExams = true;
        try
        {
            Exams.Clear();
            SelectedExam = null;

            if (course is null)
                return;

            var tasks = await _taskReadService.GetListAsync(
                includeArchived: false, CancellationToken.None);
            var exams = tasks.Where(task =>
                task.ActivityType == AcademicActivityType.Exam &&
                task.CourseId == course.Id &&
                task.Status != TaskItemStatus.Completed);

            foreach (var exam in exams)
                Exams.Add(exam);
        }
        finally
        {
            _loadingExams = false;
            OnPropertyChanged(nameof(HasExams));
        }
    }

    private async Task LoadExamTopicsAsync(TaskListItemDto? exam)
    {
        ExamTopics.Clear();
        SelectedExamTopic = null;
        HasExamCoverage = false;
        ExamCoverageText = string.Empty;
        ExamCoverageDetail = string.Empty;
        HasExamSimulation = false;
        ExamSimulationText = "Sin simulacros registrados";
        SimulationMessage = string.Empty;
        OnPropertyChanged(nameof(HasExamTopics));
        OnPropertyChanged(nameof(HasSelectedExam));

        if (exam is null)
            return;

        var detail = await _taskReadService.GetByIdAsync(exam.Id, CancellationToken.None);
        if (detail is null || SelectedExam?.Id != exam.Id)
            return;

        foreach (var topic in ExamTopicParser.ExtractTopics(detail.Subtasks.Select(s => s.Title)))
            ExamTopics.Add(topic);

        OnPropertyChanged(nameof(HasExamTopics));
        await RefreshExamCoverageAsync(exam.Id);
        await LoadExamSimulationAsync(exam.Id);
    }

    private async Task RefreshExamCoverageAsync(Guid examTaskId)
    {
        var coverage = await _examPreparationService.GetCoverageAsync(examTaskId);
        if (coverage is null || SelectedExam?.Id != examTaskId)
            return;

        HasExamCoverage = coverage.HasTopics || coverage.HasSessions;
        ExamCoverageText = coverage.HasTopics
            ? $"Cobertura: {coverage.CoveredTopics}/{coverage.TotalTopics} temas ({coverage.CoverageRate:P0})"
            : $"Sin temas definidos · {coverage.SessionsCount} sesión(es)";
        ExamCoverageDetail = BuildCoverageDetail(coverage);
    }

    private async Task LoadExamSimulationAsync(Guid examTaskId)
    {
        var summary = await _examSimulationService.GetSummaryAsync(examTaskId);
        if (SelectedExam?.Id != examTaskId)
            return;

        HasExamSimulation = summary.HasData;
        if (!summary.HasData)
        {
            ExamSimulationText = "Sin simulacros registrados";
            return;
        }

        var parts = new List<string>
        {
            $"{summary.Attempts} simulacro(s)",
            $"último {summary.LastScore}%"
        };

        if (summary.BestScore.HasValue)
            parts.Add($"mejor {summary.BestScore}%");

        if (summary.Improvement.HasValue && summary.Improvement.Value != 0)
            parts.Add($"{summary.Improvement.Value:+0;-0;0} pts desde el primero");

        ExamSimulationText = string.Join(" · ", parts);
    }

    [RelayCommand]
    private async Task SaveSimulationAsync()
    {
        if (IsBusy)
            return;

        if (SelectedExam is null)
        {
            SimulationMessage = "Selecciona un examen antes de registrar el simulacro.";
            return;
        }

        if (!int.TryParse(SimulationScoreText, out var score) || score is < 0 or > 100)
        {
            SimulationMessage = "Escribe un puntaje entre 0 y 100.";
            return;
        }

        try
        {
            IsBusy = true;
            SimulationMessage = string.Empty;
            await _examSimulationService.RecordAsync(
                SelectedExam.Id, score, SimulationErrors);

            SimulationScoreText = string.Empty;
            SimulationErrors = string.Empty;
            SimulationMessage = "Simulacro guardado.";
            await LoadExamSimulationAsync(SelectedExam.Id);
            await RefreshExamCoverageAsync(SelectedExam.Id);
        }
        catch (ArgumentException ex)
        {
            SimulationMessage = ex.Message;
        }
        catch (InvalidOperationException ex)
        {
            SimulationMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static string BuildCoverageDetail(ExamCoverageDto coverage)
    {
        var parts = new List<string>();
        if (coverage.DaysUntilExam.HasValue)
        {
            parts.Add(coverage.DaysUntilExam.Value >= 0
                ? $"Faltan {coverage.DaysUntilExam} día(s)"
                : $"Venció hace {-coverage.DaysUntilExam} día(s)");
        }

        parts.Add($"{coverage.SessionsCount} sesión(es) · {coverage.TotalMinutes} min");

        if (coverage.AverageLeadTimeDays.HasValue)
            parts.Add($"Anticipación media: {coverage.AverageLeadTimeDays:0.#} día(s)");

        if (coverage.HasPendingTopics)
            parts.Add($"Siguiente: {coverage.SuggestedTopic}");

        return string.Join(" · ", parts);
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
