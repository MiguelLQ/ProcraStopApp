
using ProcraStop.Application.Tasks.DTOs;
using ProcraStop.Domain.Tasks.Entities;

namespace ProcraStop.Application.Tasks.Mappers
{
    public static class SubtaskMapper
    {
        public static Subtask ToDomain(this SubtaskDto dto)
        => new(
            dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id,
            dto.Title,
            dto.IsCompleted
        );
    }
}
