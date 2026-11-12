using ProcraStop.Domain.Academic.Entities;
using ProcraStop.Infrastructure.Academic.Persistence.Entities;

namespace ProcraStop.Infrastructure.Academic.Persistence.Mappings
{
    public static class CourseMappings
    {
        public static CourseEntity ToEntity(this Course course)
        {
            return new CourseEntity
            {
                Id = course.Id,
                Name = course.Name,
                Description = course.Description,
                Color = course.Color,
                CreatedAtUtc = course.CreatedAt,
                InterestLevel = course.InterestLevel,
                DifficultyLevel = course.DifficultyLevel,
                AppraisedAtUtc = course.AppraisedAtUtc
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
                CreatedAt = entity.CreatedAtUtc,
                InterestLevel = entity.InterestLevel,
                DifficultyLevel = entity.DifficultyLevel,
                AppraisedAtUtc = entity.AppraisedAtUtc
            };
        }
    }
}
