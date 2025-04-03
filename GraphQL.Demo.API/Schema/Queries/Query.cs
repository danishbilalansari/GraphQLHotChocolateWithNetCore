using Bogus;
using GraphQL.Demo.API.DTOs;
using GraphQL.Demo.API.Schema.Filters;
using GraphQL.Demo.API.Schema.Sorting;
using GraphQL.Demo.API.Services;
using GraphQL.Demo.API.Services.Courses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace GraphQL.Demo.API.Schema.Queries
{
    /*
    // Bogus is used to generate fake data.
    // Faker is the property of bogus used to generate the data.
    
    public class Query
    {
        private readonly Faker<InstructorType> _instructorFaker;
        private readonly Faker<StudentType> _studentFaker;
        private readonly Faker<CourseType> _courseFaker;

        /// <summary>
        /// This will initialize the instructor, student, course faker using bogus
        /// </summary>
        public Query()
        {
            _instructorFaker = new Faker<InstructorType>()
                .RuleFor(c => c.Id, f => Guid.NewGuid())
                .RuleFor(c => c.FirstName, f => f.Name.FirstName())
                .RuleFor(c => c.LastName, f => f.Name.LastName())
                .RuleFor(c => c.Salary, f => f.Random.Double(0, 100000));

            _studentFaker = new Faker<StudentType>()
               .RuleFor(c => c.Id, f => Guid.NewGuid())
               .RuleFor(c => c.FirstName, f => f.Name.FirstName())
               .RuleFor(c => c.LastName, f => f.Name.LastName())
               .RuleFor(c => c.GPA, f => f.Random.Double(1, 4));

            _courseFaker = new Faker<CourseType>()
                .RuleFor(c => c.Id, f => Guid.NewGuid())
                .RuleFor(c => c.Name, f => f.Name.JobTitle())
                .RuleFor(c => c.Subject, f => f.PickRandom<Subject>())
                .RuleFor(c => c.Instructor, f => _instructorFaker.Generate())
                .RuleFor(c => c.Students, f => _studentFaker.Generate(3));
        }

        /// <summary>
        /// This will return the course list
        /// </summary>
        /// <returns></returns>
        public IEnumerable<CourseType> GetCourses()
        {
            return _courseFaker.Generate(5);
        }

        /// <summary>
        /// Get the single course type
        /// </summary>
        /// <param name="id">The id of the course</param>
        /// <returns>The course type</returns>
        public CourseType GetCourseById(Guid id)
        {
            CourseType course = _courseFaker.Generate();
            course.Id = id;
            return course;
        }

        /// <summary>
        /// This will deprecate the instructions from the GraphQL.
        /// </summary>
        [GraphQLDeprecated("This query is deprecated.")]
        public string Instructions => "Hello GraphQL";
        }
        */

    public class Query
    {
        private readonly CoursesRepository _coursesRepository;

        public Query(CoursesRepository coursesRepository)
        {
            _coursesRepository = coursesRepository;
        }

        public async Task<IEnumerable<CourseType>> GetCourses()
        {
            IEnumerable<CourseDTO> courseDTOs = await _coursesRepository.GetAll();

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

        public async Task<CourseType> GetCourseByIdAsync(Guid id)
        {
            CourseDTO courseDTO = await _coursesRepository.GetById(id);

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