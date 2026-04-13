

using ProcraStop.Modules.Tasks.Domain.Entities;

namespace ProcraStop.Modules.Tasks.Infraestructure.Persistence
{
    public static class CourseMappings
    {
        // este método es para convertir un Course a un CourseEntity, y el otro método es para convertir un CourseEntity a un Course
        public static CourseEntity ToEntity(this Course course)
        {
            return new CourseEntity
            {
                Id = course.Id,
                Name = course.Name,
                Description = course.Description,
                Color = course.Color,
                CreatedAtUtc = course.CreatedAt
            };
        }

        public static Course ToDomain(this CourseEntity entity)
        {
            return new Course
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                Color = entity.Color,
                CreatedAt = entity.CreatedAtUtc
            };
        }
    }
}
