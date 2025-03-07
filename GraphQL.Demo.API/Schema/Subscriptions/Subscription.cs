using GraphQL.Demo.API.Schema.Mutations;
using HotChocolate.Execution;
using HotChocolate.Subscriptions;

namespace GraphQL.Demo.API.Schema.Subscriptions
{
    public class Subscription
    {
        /// <summary>
        /// A subscribe method whenever a course result created event
        /// </summary>
        /// <param name="course">The Course Type object</param>
        /// <returns>The course type</returns>
        [Subscribe]
        public CourseResult CourseCreated([EventMessage] CourseResult course) => course;

        /// <summary>
        /// A subscriber method whenever a course result is updated
        /// When a client subscribes to a specific event or topic (like "course updated"), the GraphQL server 
        /// will call this method to resolve and send data to the client.
        /// </summary>
        /// <param name="courseId">The course id</param>
        /// <param name="topicEventReceiver">
        /// Service is part of HotChocolate. While the ITopicEventReceiver is an interface that is used for receiving events
        /// to subscribers in a GraphQL Subscription. It allows to receive events to specific topic.
        /// </param>
        /// <returns></returns>
        [SubscribeAndResolve]
        public ValueTask<ISourceStream<CourseResult>> CourseUpdated(Guid courseId, [Service] ITopicEventReceiver topicEventReceiver) 
        {
            string topicName = $"{courseId}_{nameof(Subscription.CourseUpdated)}";
            return topicEventReceiver.SubscribeAsync<CourseResult>(topicName);
        }
    }
}
