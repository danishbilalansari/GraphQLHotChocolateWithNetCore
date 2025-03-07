using Bogus.DataSets;
using GraphQL.Demo.API.Schema.Queries;
using GraphQL.Demo.API.Schema.Subscriptions;
using HotChocolate.Subscriptions;
using Microsoft.AspNetCore.Connections.Features;

namespace GraphQL.Demo.API.Schema.Mutations
{
    /// <summary>
    /// Mutation is used for create, update and delete in GraphQL.
    /// </summary>
    public class Mutation
    {
        private readonly List<CourseResult> _courses;

        public Mutation()
        {
            _courses = new List<CourseResult>();
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
        public async Task<CourseResult> CreateCourse(CourseInputType courseInputType, [Service] ITopicEventSender topicEventSender)
        {
            CourseResult courseType = new CourseResult()
            {
                Id = Guid.NewGuid(),
                Name = courseInputType.Name,
                Subject = courseInputType.Subject,
                InstructorId = courseInputType.InstructorId
            };

            _courses.Add(courseType);
            await topicEventSender.SendAsync(nameof(Subscription.CourseCreated), courseType);

            return courseType;
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
        public async Task<CourseResult> UpdateCourse(Guid id, CourseInputType courseInputType, [Service] ITopicEventSender topicEventSender)
        {
            CourseResult course = _courses.FirstOrDefault(c => c.Id == id);

            if (course == null) 
            {
                throw new GraphQLException(new Error("Course not found.", "COURSE_NOT_FOUND"));
            }

            course.Name = courseInputType.Name;
            course.Subject = courseInputType.Subject;
            course.InstructorId = courseInputType.InstructorId;

            string updatedCourseTopic = $"{course.Id}_{nameof(Subscription.CourseUpdated)}";
            await topicEventSender.SendAsync(updatedCourseTopic, course);

            return course;
        }

        /// <summary>
        /// Deletes the course type
        /// </summary>
        /// <param name="id">The id of the course id</param>
        /// <returns>Returns true if deleted successfully else falsef</returns>
        public bool DeleteCourse(Guid id) 
        {
            return _courses.RemoveAll(c => c.Id == id) >= 1;
        }
    }
}
