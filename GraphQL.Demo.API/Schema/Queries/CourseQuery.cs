using GraphQL.Demo.API.DTOs;
using GraphQL.Demo.API.Schema.Filters;
using GraphQL.Demo.API.Schema.Sorting;
using GraphQL.Demo.API.Services;
using GraphQL.Demo.API.Services.Courses;
using Microsoft.EntityFrameworkCore;

namespace GraphQL.Demo.API.Schema.Queries
{
    [ExtendObjectType(typeof(Query))]
    public class CourseQuery
    {
        private readonly CoursesRepository _coursesRepository;

        public CourseQuery(CoursesRepository coursesRepository)
        {
            _coursesRepository = coursesRepository;
        }

        /// <summary>
        /// Retrieves all courses from the repository and maps them to a collection of <see cref="CourseType"/>.
        /// </summary>
        /// <returns>
        /// An enumerable collection of <see cref="CourseType"/> objects representing all available courses.
        /// </returns>
        public async Task<IEnumerable<CourseType>> GetCourses()
        {
            // Retrieve all courses from the repository as DTOs
            IEnumerable<CourseDTO> courseDTOs = await _coursesRepository.GetAll();

            // Map each CourseDTO to a GraphQL CourseType and return the result
            return courseDTOs.Select(c => new CourseType()
            {
                Id = c.Id,
                Name = c.Name,
                Subject = c.Subject,
                InstructorId = c.InstructorId
            });
        }

        /// <summary>
        /// Retrieves a paginated and filterable list of courses from the database.
        /// The ordering matters, paging > projections > filtering > sorting
        /// </summary>
        /// <remarks>
        /// - Uses GraphQL's built-in pagination via the [UsePaging] attribute to enable efficient data retrieval.
        /// - Supports dynamic filtering with [UseFiltering], utilizing a custom filter type (CourseFilterType) 
        ///   to control filtering behavior and exclude specific fields.
        /// - Queries the Courses table and maps each course entity to a GraphQL DTO (CourseType).
        /// - The IQueryable return type allows for deferred execution, ensuring that pagination, filtering, and sorting
        ///   are applied efficiently at the database level, reducing unnecessary data transfer.
        /// </remarks>
        /// <param name="contextFactory">The factory used to create a new instance of SchoolDbContext.</param>
        /// <returns>
        /// A queryable collection of CourseType objects, enabling efficient pagination and filtering.
        /// </returns>
        [UsePaging(IncludeTotalCount = true, DefaultPageSize = 10)]
        [UseProjection]
        [UseFiltering(typeof(CourseFilterType))] // Applies custom filtering rules defined in CourseFilterType
        [UseSorting(typeof(CourseSortType))] // Applies custom sorting rules defined in CourseSortType
        public IQueryable<CourseType> GetPaginatedCourses([Service] IDbContextFactory<SchoolDbContext> contextFactory)
        {
            SchoolDbContext context = contextFactory.CreateDbContext();

            return context.Courses.Select(c => new CourseType()
            {
                Id = c.Id,
                Name = c.Name,
                Subject = c.Subject,
                InstructorId = c.InstructorId,
                CreatorId = c.CreatorId
            });
        }

        /// <summary>
        /// Retrieves a course by its unique identifier from the repository and maps it to a <see cref="CourseType"/>.
        /// </summary>
        /// <param name="id">The unique identifier of the course.</param>
        /// <returns>
        /// Returns a <see cref="CourseType"/> object that represents the course details.
        /// </returns>
        public async Task<CourseType> GetCourseByIdAsync(Guid id)
        {
            // Fetch the course data transfer object from the repository using the provided course ID
            CourseDTO courseDTO = await _coursesRepository.GetById(id);

            // Map the DTO to a GraphQL CourseType object
            return new CourseType()
            {
                Id = courseDTO.Id,
                Name = courseDTO.Name,
                Subject = courseDTO.Subject,
                InstructorId = courseDTO.InstructorId
            };
        }
    }
}
