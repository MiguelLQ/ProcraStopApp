
using ProcraStop.Application.Academic.Abstractions;

namespace ProcraStop.Application.Academic.Commands.UpdateCourse
{
    public sealed class UpdateCourseCommandHandler
    {
        private readonly ICourseRepository _repository;

        public UpdateCourseCommandHandler(ICourseRepository repository)
        {
            _repository = repository;
        }

        public async Task HandleAsync(
        UpdateCourseCommand command,
        CancellationToken ct = default)
        {
                var course = await _repository.GetByIdAsync(command.Id, ct);

                if (course is null)
                    throw new Exception("Course not found");

                // modificar el existente
                course.Name = command.Name;
                course.Description = command.Description;
                course.Color = command.Color;

                await _repository.UpdateAsync(course, ct);
        }
    }
}
