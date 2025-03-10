﻿using Bogus;

namespace GraphQL.Demo.API.Schema.Queries
{
    /*
    Bogus is used to generate fake data.
    Faker is the property of bogus used to generate the data.
    */
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
}
