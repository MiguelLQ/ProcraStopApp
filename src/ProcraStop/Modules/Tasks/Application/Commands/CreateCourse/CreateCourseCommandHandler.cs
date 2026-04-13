using ProcraStop.Modules.Tasks.Application.Abstractions;
using ProcraStop.Modules.Tasks.Domain.Entities;


namespace ProcraStop.Modules.Tasks.Application.Commands.CreateCourse
{
    public sealed class CreateCourseCommandHandler
    {
        private readonly ICourseRepository _repository;

        public CreateCourseCommandHandler(ICourseRepository repository)
        {
            _repository = repository;
        }

        public async Task<Guid> HandleAsync(
            CreateCourseCommand command,
            CancellationToken ct = default)
        {
            var course = new Course
            {
                Id = Guid.NewGuid(),
                Name = command.Name,
                Description = command.Description,
                Color = command.Color,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(course, ct);

            return course.Id;
        }
    }
}
