

using ProcraStop.Application.Tasks.Abstractions;

namespace ProcraStop.Application.Tasks.Commands.DeleteCourse
{
    public sealed class DeleteCourseCommandHandler
    {
        private readonly ICourseRepository _repository;

        public DeleteCourseCommandHandler(ICourseRepository repository)
        {
            _repository = repository;
        }

        public async Task HandleAsync(
            DeleteCourseCommand command,
            CancellationToken ct = default)
        {
            var course = await _repository.GetByIdAsync(command.Id, ct);

            if (course is null)
                return; // o lanzar excepción si prefieres

            await _repository.DeleteAsync(command.Id, ct);
        }
    }
}
