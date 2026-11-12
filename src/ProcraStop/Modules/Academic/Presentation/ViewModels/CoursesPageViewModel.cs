using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProcraStop.Application.Academic.DTOs;
using ProcraStop.Application.Academic.Facade;
using ProcraStop.Modules.Academic.Presentation.Models;
using ProcraStop.Modules.Tasks.Presentation.Models;
using System.Collections.ObjectModel;

namespace ProcraStop.Modules.Academic.Presentation.ViewModels
{
    public partial class CoursesPageViewModel :ObservableObject
    {
        private readonly ICoursesFacade _coursesFacade;

        public CoursesPageViewModel(ICoursesFacade coursesFacade)
        {
            _coursesFacade = coursesFacade;
            ColorOptions = new ObservableCollection<ColorOption>
            {
                new() { Hex = "#EF4444" }, // rojo
                new() { Hex = "#F97316" }, // naranja
                new() { Hex = "#F59E0B" }, // amarillo
                new() { Hex = "#84CC16" }, // lima
                new() { Hex = "#10B981" }, // verde
                new() { Hex = "#06B6D4" }, // cyan
                new() { Hex = "#3B82F6" }, // azul
                new() { Hex = "#6366F1" }, // índigo
                new() { Hex = "#8B5CF6" }, // violeta
                new() { Hex = "#D946EF" }, // fucsia
                new() { Hex = "#EC4899" }, // rosa
                new() { Hex = "#64748B" }  // gris
            };
        }

        // =========================
        //  LISTA
        // =========================

        [ObservableProperty]
        private ObservableCollection<CourseDto> courses = new();

        [ObservableProperty]
        private CourseDto? selectedCourse;

        [ObservableProperty]
        private bool isLoading;

        // =========================
        //  FORM (CREATE / UPDATE)
        // =========================

        [ObservableProperty]
        private string name = string.Empty;

        [ObservableProperty]
        private string? description;

        [ObservableProperty]
        private string? selectedColor;

        // D1 EPA: valoracion opcional por curso (1..5), no se pregunta por tarea.
        public IReadOnlyList<AppraisalOption> AppraisalOptions { get; } = AppraisalOption.All;

        [ObservableProperty]
        private AppraisalOption selectedInterest = AppraisalOption.All[0];

        [ObservableProperty]
        private AppraisalOption selectedDifficulty = AppraisalOption.All[0];

        [ObservableProperty]
        private bool isEditing;
        [RelayCommand]
        private void SetColor(string color)
        {
            SelectedColor = color;
        }
        [ObservableProperty]
        private ObservableCollection<ColorOption> colorOptions = new();
        // =========================
        // LOAD
        // =========================

        public async Task LoadAsync()
        {
            var list = await _coursesFacade.GetCoursesAsync();

            Courses.Clear();

            foreach (var item in list)
                Courses.Add(item);
        }

        // =========================
        // CREATE / UPDATE
        // =========================

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(Name))
                return;

            if (IsEditing && SelectedCourse is not null)
            {
                // UPDATE
                var request = new UpdateCourseRequest
                {
                    Id = SelectedCourse.Id,
                    Name = Name.Trim(),
                    Description = Description,
                    Color = SelectedColor,
                    InterestLevel = SelectedInterest.Value,
                    DifficultyLevel = SelectedDifficulty.Value
                };

                await _coursesFacade.UpdateCourseAsync(request);
                UpsertCourse(new CourseDto
                {
                    Id = request.Id,
                    Name = request.Name,
                    Description = request.Description,
                    Color = request.Color,
                    InterestLevel = request.InterestLevel,
                    DifficultyLevel = request.DifficultyLevel
                });
            }
            else
            {
                //  CREATE
                var request = new CreateCourseRequest
                {
                    Name = Name.Trim(),
                    Description = Description,
                    Color = SelectedColor,
                    InterestLevel = SelectedInterest.Value,
                    DifficultyLevel = SelectedDifficulty.Value
                };

                var id = await _coursesFacade.CreateCourseAsync(request);
                UpsertCourse(new CourseDto
                {
                    Id = id,
                    Name = request.Name,
                    Description = request.Description,
                    Color = request.Color,
                    InterestLevel = request.InterestLevel,
                    DifficultyLevel = request.DifficultyLevel
                });
            }

            ResetForm();
        }

        // =========================
        // EDITAR
        // =========================

        [RelayCommand]
        private void Edit(CourseDto course)
        {
            SelectedCourse = course;

            Name = course.Name;
            Description = course.Description;
            SelectedColor = course.Color;
            SelectedInterest = AppraisalOption.Find(course.InterestLevel);
            SelectedDifficulty = AppraisalOption.Find(course.DifficultyLevel);

            IsEditing = true;
        }

        // =========================
        // DELETE
        // =========================

        [RelayCommand]
        private async Task DeleteAsync(CourseDto course)
        {
            if (course is null)
                return;

            await _coursesFacade.DeleteCourseAsync(course.Id);
            Courses.Remove(course);
        }

        // =========================
        // CANCELAR EDICIÓN
        // =========================

        [RelayCommand]
        private void CancelEdit()
        {
            ResetForm();
        }

        // =========================
        // RESET FORM
        // =========================

        private void ResetForm()
        {
            Name = string.Empty;
            Description = null;
            SelectedColor = null;
            SelectedInterest = AppraisalOption.All[0];
            SelectedDifficulty = AppraisalOption.All[0];
            SelectedCourse = null;
            IsEditing = false;
        }

        private void UpsertCourse(CourseDto course)
        {
            var existing = Courses.FirstOrDefault(x => x.Id == course.Id);
            if (existing is not null)
                Courses.Remove(existing);

            var insertAt = 0;
            while (insertAt < Courses.Count &&
                   string.Compare(
                       Courses[insertAt].Name,
                       course.Name,
                       StringComparison.CurrentCultureIgnoreCase) <= 0)
            {
                insertAt++;
            }

            Courses.Insert(insertAt, course);
        }
        [RelayCommand]
        private void SelectColor(ColorOption color)
        {
            foreach (var c in ColorOptions)
                c.IsSelected = false;

            color.IsSelected = true;

            SelectedColor = color.Hex;
        }
    }
}
