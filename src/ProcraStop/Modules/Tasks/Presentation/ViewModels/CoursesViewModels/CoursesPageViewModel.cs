using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProcraStop.Modules.Tasks.Application.DTOs;
using ProcraStop.Modules.Tasks.Application.Facade;
using ProcraStop.Modules.Tasks.Presentation.Models;
using System.Collections.ObjectModel;

namespace ProcraStop.Modules.Tasks.Presentation.ViewModels.CoursesViewModels
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
                    Color = SelectedColor
                };

                await _coursesFacade.UpdateCourseAsync(request);
                await LoadAsync(); //recargar lista
            }
            else
            {
                //  CREATE
                var request = new CreateCourseRequest
                {
                    Name = Name.Trim(),
                    Description = Description,
                    Color = SelectedColor
                };

                await _coursesFacade.CreateCourseAsync(request);
            }

            await LoadAsync();
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

            await LoadAsync();
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
            SelectedCourse = null;
            IsEditing = false;
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
