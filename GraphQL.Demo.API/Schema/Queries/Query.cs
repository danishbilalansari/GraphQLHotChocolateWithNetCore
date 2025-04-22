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
        /// <summary>
        /// Performs a search across multiple entities (courses and instructors) by a keyword term.
        /// </summary>
        /// <param name="term">The search term to look for in course names and instructor names.</param>
        /// <param name="contextFactory">Factory used to create a new instance of the SchoolDbContext.</param>
        /// <returns>
        /// A combined list of results (courses and instructors) that match the search term,
        /// returned as a collection of <see cref="ISearchResultType"/>.
        /// </returns>
        public async Task<IEnumerable<ISearchResultType>> Search(string term, [Service] IDbContextFactory<SchoolDbContext> contextFactory)
        {
            // Create a new instance of SchoolDbContext using the factory
            SchoolDbContext context = contextFactory.CreateDbContext();

            // Search for courses where the course name contains the search term
            IEnumerable<CourseType> courses = await context.Courses
                .Where(c => c.Name.Contains(term))
                .Select(c => new CourseType()
                {
                    Id = c.Id,
                    Name = c.Name,
                    Subject = c.Subject,
                    InstructorId = c.InstructorId,
                    CreatorId = c.CreatorId
                }).ToListAsync();

            // Search for instructors where either the first or last name contains the search term
            IEnumerable<InstructorType> instructors = await context.Instructors
                .Where(i => i.FirstName.Contains(term) || i.LastName.Contains(term))
                .Select(i => new InstructorType()
                {
                    Id = i.Id,
                    FirstName = i.FirstName,
                    LastName = i.LastName,
                    Salary = i.Salary
                }).ToListAsync();

            // Combine both result sets into a single list of ISearchResultType (shared interface between CourseType and InstructorType)
            return new List<ISearchResultType>()
                .Concat(courses)
                .Concat(instructors);
        }
    }
}