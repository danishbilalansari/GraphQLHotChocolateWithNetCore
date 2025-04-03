using FirebaseAdmin.Auth;
using GraphQL.Demo.API.DataLoaders;
using GraphQL.Demo.API.DTOs;
using GraphQL.Demo.API.Models;
using HotChocolate.Types;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace GraphQL.Demo.API.Schema.Queries
{
    public class CourseType
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Subject Subject { get; set; }

        /*        
        - Ensures InstructorId is included in the database query even if not explicitly requested.
        - Prevents errors when resolving the Instructor field using DataLoader.
        - Optimizes database queries by ensuring necessary fields are always fetched.
        - This is used to prevent Underfetching.
        - Overfetching happens when a query retrieves more data than necessary, leading to inefficient database usage and increased response size.
        - Underfetching occurs when a query does not fetch the necessary fields, causing missing data and potentially requiring additional queries.
        */
        [IsProjected(true)]
        public Guid InstructorId { get; set; }

        /// <summary>
        /// The Instructor DataLoader is used to load the instructor data by only querying to db one time.
        /// </summary>
        /// <param name="instructorsDataLoader">The instructorsDataLoader.</param>
        /// <returns>Returns the instructor type.</returns>
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

        [IsProjected(true)]
        public string CreatorId { get; set; }

        /// <summary>
        /// Resolves the creator of the course by fetching user details from Firebase Authentication.
        /// </summary>
        /// <param name="userDataLoader">The data loader used to batch and cache user retrieval requests.</param>
        /// <returns>
        /// Returns the <see cref="UserType"/> representing the creator of the course, or null if CreatorId is not set.
        /// </returns>
        public async Task<UserType> Creator([Service] UserDataLoader userDataLoader)
        {
            // If CreatorId is null, return null immediately to avoid unnecessary API calls.
            if (CreatorId == null) return null;

            // Fetches the user details from Firebase Authentication using the CreatorId.
            UserRecord user = await FirebaseAuth.DefaultInstance.GetUserAsync(CreatorId);

            // Uses the UserDataLoader to efficiently batch and cache user retrieval requests.
            return await userDataLoader.LoadAsync(CreatorId, CancellationToken.None);
        }
    }
}
