using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProcraStop.Application.Academic.DTOs;
using ProcraStop.Application.Academic.Facade;
using ProcraStop.Modules.Academic.Presentation.Models;
using ProcraStop.Shared.Services;
using ProcraStop.Shared.UI.Components;
using System.Collections.ObjectModel;
using System.Globalization;

namespace ProcraStop.Modules.Academic.Presentation.ViewModels;

public partial class SchedulePageViewModel : ObservableObject
{
    private readonly IAcademicScheduleFacade _scheduleFacade;
    private readonly ICoursesFacade _coursesFacade;
    private readonly ISnackbarService _snackbar;

    public SchedulePageViewModel(
        IAcademicScheduleFacade scheduleFacade,
        ICoursesFacade coursesFacade,
        ISnackbarService snackbar)
    {
        _scheduleFacade = scheduleFacade;
        _coursesFacade = coursesFacade;
        _snackbar = snackbar;
        BuildWeek();
    }

    public ObservableCollection<ScheduleDayOption> Days { get; } = new();
    public ObservableCollection<ClassScheduleDto> DayClasses { get; } = new();
    public ObservableCollection<CourseDto> Courses { get; } = new();
    public ObservableCollection<int> ReminderOptions { get; } = new(new[] { 0, 5, 10, 15, 30, 45, 60 });

    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private bool isEditing;
    [ObservableProperty] private Guid? editingScheduleId;
    [ObservableProperty] private ScheduleDayOption? selectedDay;
    [ObservableProperty] private CourseDto? selectedCourse;
    [ObservableProperty] private TimeSpan startTime = new(8, 0, 0);
    [ObservableProperty] private TimeSpan endTime = new(9, 0, 0);
    [ObservableProperty] private string? classroom;
    [ObservableProperty] private string? instructor;
    [ObservableProperty] private int selectedReminder = 15;
    [ObservableProperty] private string selectedDayTitle = "Hoy";
    [ObservableProperty] private string daySummary = "Sin clases registradas";

    public bool CanSave => SelectedCourse is not null && EndTime > StartTime;

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            await LoadCoursesAsync();

            if (SelectedDay is null)
                SelectDay(Days.FirstOrDefault(x => x.DayOfWeek == DateTime.Today.DayOfWeek) ?? Days.FirstOrDefault());

            await LoadDayScheduleAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SelectDayAsync(ScheduleDayOption? day)
    {
        SelectDay(day);
        await LoadDayScheduleAsync();
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        if (SelectedCourse is null || SelectedDay is null)
            return;

        if (EndTime <= StartTime)
        {
            await _snackbar.ShowErrorAsync("La hora de fin debe ser posterior al inicio.", SnackbarType.Error);
            return;
        }

        try
        {
            if (IsEditing && EditingScheduleId.HasValue)
            {
                await _scheduleFacade.UpdateScheduleAsync(new UpdateClassScheduleRequest
                {
                    Id = EditingScheduleId.Value,
                    CourseId = SelectedCourse.Id,
                    DayOfWeek = SelectedDay.DayOfWeek,
                    StartTime = StartTime,
                    EndTime = EndTime,
                    Classroom = Classroom,
                    Instructor = Instructor,
                    ReminderMinutesBefore = SelectedReminder
                });
            }
            else
            {
                await _scheduleFacade.CreateScheduleAsync(new CreateClassScheduleRequest
                {
                    CourseId = SelectedCourse.Id,
                    DayOfWeek = SelectedDay.DayOfWeek,
                    StartTime = StartTime,
                    EndTime = EndTime,
                    Classroom = Classroom,
                    Instructor = Instructor,
                    ReminderMinutesBefore = SelectedReminder
                });
            }

            ResetForm();
            await LoadDayScheduleAsync();
            await _snackbar.ShowAsync("Horario actualizado.", SnackbarType.Success);
        }
        catch (Exception ex)
        {
            await _snackbar.ShowErrorAsync(ex.Message, SnackbarType.Error);
        }
    }

    [RelayCommand]
    private void Edit(ClassScheduleDto? item)
    {
        if (item is null)
            return;

        var day = Days.FirstOrDefault(x => x.DayOfWeek == item.DayOfWeek);
        SelectDay(day);
        SelectedCourse = Courses.FirstOrDefault(x => x.Id == item.CourseId);
        StartTime = item.StartTime;
        EndTime = item.EndTime;
        Classroom = item.Classroom;
        Instructor = item.Instructor;
        SelectedReminder = item.ReminderMinutesBefore;
        EditingScheduleId = item.Id;
        IsEditing = true;
    }

    [RelayCommand]
    private async Task DeleteAsync(ClassScheduleDto? item)
    {
        if (item is null)
            return;

        await _scheduleFacade.DeleteScheduleAsync(item.Id);
        if (EditingScheduleId == item.Id)
            ResetForm();

        await LoadDayScheduleAsync();
        await _snackbar.ShowAsync("Clase retirada del horario.", SnackbarType.Success);
    }

    [RelayCommand]
    private void CancelEdit()
    {
        ResetForm();
    }

    partial void OnSelectedCourseChanged(CourseDto? value) => SaveCommand.NotifyCanExecuteChanged();
    partial void OnStartTimeChanged(TimeSpan value) => SaveCommand.NotifyCanExecuteChanged();
    partial void OnEndTimeChanged(TimeSpan value) => SaveCommand.NotifyCanExecuteChanged();

    private async Task LoadCoursesAsync()
    {
        if (Courses.Count > 0)
            return;

        var courses = await _coursesFacade.GetCoursesAsync();
        Courses.Clear();
        foreach (var course in courses)
            Courses.Add(course);

        SelectedCourse ??= Courses.FirstOrDefault();
    }

    private async Task LoadDayScheduleAsync()
    {
        if (SelectedDay is null)
            return;

        var items = await _scheduleFacade.GetScheduleAsync(SelectedDay.DayOfWeek);
        DayClasses.Clear();
        foreach (var item in items)
            DayClasses.Add(item);

        SelectedDayTitle = SelectedDay.DayOfWeek == DateTime.Today.DayOfWeek
            ? "Clases de hoy"
            : $"Clases del {SelectedDay.DisplayName}";

        DaySummary = DayClasses.Count == 0
            ? "Sin clases registradas"
            : $"{DayClasses.Count} clase{(DayClasses.Count == 1 ? string.Empty : "s")} programada{(DayClasses.Count == 1 ? string.Empty : "s")}";
    }

    private void SelectDay(ScheduleDayOption? day)
    {
        if (day is null)
            return;

        foreach (var option in Days)
            option.IsSelected = option.DayOfWeek == day.DayOfWeek;

        SelectedDay = day;
    }

    private void ResetForm()
    {
        EditingScheduleId = null;
        IsEditing = false;
        Classroom = null;
        Instructor = null;
        SelectedReminder = 15;
        StartTime = new(8, 0, 0);
        EndTime = new(9, 0, 0);
        SelectedCourse = Courses.FirstOrDefault();
    }

    private void BuildWeek()
    {
        var today = DateTime.Today;
        var mondayOffset = ((int)today.DayOfWeek + 6) % 7;
        var start = today.AddDays(-mondayOffset);
        var culture = new CultureInfo("es-ES");

        Days.Clear();
        foreach (var offset in Enumerable.Range(0, 7))
        {
            var date = start.AddDays(offset);
            Days.Add(new ScheduleDayOption
            {
                DayOfWeek = date.DayOfWeek,
                ShortName = date.ToString("ddd", culture).TrimEnd('.'),
                DayNumber = date.Day.ToString(culture),
                DisplayName = date.ToString("dddd", culture),
                IsSelected = date.Date == today
            });
        }

        SelectedDay = Days.FirstOrDefault(x => x.IsSelected);
    }
}
