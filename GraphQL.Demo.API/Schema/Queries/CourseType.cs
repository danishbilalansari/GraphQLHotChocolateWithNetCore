using GraphQL.Demo.API.DataLoaders;
using GraphQL.Demo.API.DTOs;
using GraphQL.Demo.API.Models;

namespace GraphQL.Demo.API.Schema.Queries
{
    public class CourseType
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Subject Subject { get; set; }
        public Guid InstructorId { get; set; }

        [GraphQLNonNullType]
        public async Task<InstructorType> Instructor([Service] InstructorDataLoader instructorsDataLoader)
        {
            InstructorDTO instructorDTO = await instructorsDataLoader.LoadAsync(InstructorId, CancellationToken.None);

            return new InstructorType() 
            {
                Id= instructorDTO.Id,
                FirstName = instructorDTO.FirstName,
                LastName = instructorDTO.LastName,
                Salary = instructorDTO.Salary
            };
        }

        public IEnumerable<StudentType> Students { get; set; }
    }
}
