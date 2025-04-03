using FirebaseAdmin;
using FirebaseAdmin.Auth;
using GraphQL.Demo.API.Schema.Queries;

namespace GraphQL.Demo.API.DataLoaders
{
    /// <summary>
    /// DataLoader for batching and caching user information retrieval from Firebase Authentication.
    /// This helps in reducing the number of Firebase API calls by fetching multiple users in a single request.
    /// </summary>
    public class UserDataLoader : BatchDataLoader<string, UserType>
    {
        private readonly FirebaseAuth _fireBaseAuth;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserDataLoader"/> class.
        /// </summary>
        /// <param name="fireBaseApp">The Firebase application instance used to retrieve authentication data.</param>
        /// <param name="batchScheduler">The scheduler that batches multiple requests together for efficient processing.</param>
        /// <param name="options">Optional parameters for configuring the DataLoader behavior.</param>
        public UserDataLoader(
            FirebaseApp fireBaseApp,
            IBatchScheduler batchScheduler,
            DataLoaderOptions options = null) : base(batchScheduler, options)
        {
            // Get an instance of Firebase Authentication from the provided Firebase application.
            _fireBaseAuth = FirebaseAuth.GetAuth(fireBaseApp);
        }

        /// <summary>
        /// Loads a batch of users from Firebase Authentication based on their unique user IDs.
        /// </summary>
        /// <param name="userIds">The list of user IDs to retrieve.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>A dictionary mapping user IDs to UserType objects.</returns>
        protected override async Task<IReadOnlyDictionary<string, UserType>> LoadBatchAsync(
            IReadOnlyList<string> userIds,
            CancellationToken cancellationToken)
        {
            // Convert user IDs into Firebase UidIdentifiers for bulk retrieval.
            List<UidIdentifier> usersIdentifiers = userIds.Select(u => new UidIdentifier(u)).ToList();

            // Fetch user details from Firebase Authentication in a single batch request.
            GetUsersResult usersResult = await _fireBaseAuth.GetUsersAsync(usersIdentifiers);

            // Convert Firebase user data into UserType objects and return as a dictionary.
            return usersResult.Users.Select(u => new UserType()
            {
                Id = u.Uid,
                Username = u.DisplayName,
                PhotoUrl = u.PhotoUrl
            }).ToDictionary(u => u.Id);
        }
    }
}
