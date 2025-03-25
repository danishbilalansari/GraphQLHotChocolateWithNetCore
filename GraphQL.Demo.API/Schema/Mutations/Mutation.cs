using FirebaseAdminAuthentication.DependencyInjection.Models;
using GraphQL.Demo.API.DTOs;
using GraphQL.Demo.API.Schema.Subscriptions;
using GraphQL.Demo.API.Services.Courses;
using HotChocolate.Authorization;
using HotChocolate.Subscriptions;
using System.Security.Claims;

namespace GraphQL.Demo.API.Schema.Mutations
{
    /// <summary>
    /// Mutation is used for create, update and delete in GraphQL.
    /// </summary>
    public class Mutation
    {
        private readonly CoursesRepository _coursesRepository;

        public Mutation(CoursesRepository coursesRepository)
        {
            _coursesRepository = coursesRepository;
        }

        /// <summary>
        /// Create the course in GraphQL
        /// </summary>
        /// <param name="courseInputType">The course input type object</param>
        /// <param name="topicEventSender">
        /// Service is part of HotChocolate. While the ITopicEventSender is an interface that is used for publishing events
        /// to subscribers in a GraphQL Subscription. It allows to send events to specific topic.
        /// </param>
        /// <returns>Returns the created course result</returns>
        [Authorize]
        public async Task<CourseResult> CreateCourse(CourseInputType courseInputType,
            [Service] ITopicEventSender topicEventSender,
            ClaimsPrincipal claimsPrincipal)
        {
            string userId = claimsPrincipal.FindFirstValue(FirebaseUserClaimType.ID);
            string email = claimsPrincipal.FindFirstValue(FirebaseUserClaimType.EMAIL);
            string username = claimsPrincipal.FindFirstValue(FirebaseUserClaimType.USERNAME);
            string verified = claimsPrincipal.FindFirstValue(FirebaseUserClaimType.EMAIL_VERIFIED);

            CourseDTO courseDTO = new CourseDTO()
            {
                Name = courseInputType.Name,
                Subject = courseInputType.Subject,
                InstructorId = courseInputType.InstructorId,
            };

            courseDTO = await _coursesRepository.Create(courseDTO);

            CourseResult course = new CourseResult()
            {
                Id = courseDTO.Id,
                Name = courseDTO.Name,
                Subject = courseDTO.Subject,
                InstructorId = courseDTO.InstructorId
            };

            await topicEventSender.SendAsync(nameof(Subscription.CourseCreated), course);

            return course;
        }

        /// <summary>
        /// Update the course in GraphQL
        /// </summary>
        /// <param name="id">The id of the course</param>
        /// <param name="courseInputType">The course input type object</param>
        /// <param name="topicEventSender">
        /// Service is part of HotChocolate. While the ITopicEventSender is an interface that is used for publishing events
        /// to subscribers in a GraphQL Subscription. It allows to send events to specific topic.
        /// </param>
        /// <returns>Returns the updated course result</returns>
        /// <exception cref="Exception">If course not found returns exception</exception>
        [Authorize]
        public async Task<CourseResult> UpdateCourse(Guid id, CourseInputType courseInputType, [Service] ITopicEventSender topicEventSender)
        {
            CourseDTO courseDTO = await _coursesRepository.GetById(id);

            if (courseDTO == null)
            {
                throw new GraphQLException(new Error("Course not found.", "COURSE_NOT_FOUND"));
            }

            courseDTO.Name = courseInputType.Name;
            courseDTO.Subject = courseInputType.Subject;
            courseDTO.InstructorId = courseInputType.InstructorId;

            courseDTO = await _coursesRepository.Update(courseDTO);

            CourseResult course = new CourseResult()
            {
                Id = courseDTO.Id,
                Name = courseDTO.Name,
                Subject = courseDTO.Subject,
                InstructorId = courseDTO.InstructorId
            };

            string updatedCourseTopic = $"{course.Id}_{nameof(Subscription.CourseUpdated)}";
            await topicEventSender.SendAsync(updatedCourseTopic, course);

            return course;
        }

        /// <summary>
        /// Deletes the course type
        /// </summary>
        /// <param name="id">The id of the course id</param>
        /// <returns>Returns true if deleted successfully else falsef</returns>
        [Authorize]
        public async Task<bool> DeleteCourse(Guid id)
        {
            try
            {
                return await _coursesRepository.Delete(id);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
