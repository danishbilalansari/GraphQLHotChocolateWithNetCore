using FirebaseAdmin;
using FirebaseAdminAuthentication.DependencyInjection.Extensions;
using FirebaseAdminAuthentication.DependencyInjection.Models;
using GraphQL.Demo.API.DataLoaders;
using GraphQL.Demo.API.Schema.Mutations;
using GraphQL.Demo.API.Schema.Queries;
using GraphQL.Demo.API.Schema.Subscriptions;
using GraphQL.Demo.API.Services;
using GraphQL.Demo.API.Services.Courses;
using GraphQL.Demo.API.Services.Instructors;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration; 

services.AddControllers();

/*
Configures the GraphQL server with essential features:
- Query, Mutation, and Subscription types for fetching, modifying, and real-time updates.
- In-memory subscriptions to enable real-time event handling without an external message broker.
- DbContext factory registration to provide efficient database context instances per request, avoiding scope issues.
- Data loader integration to optimize database access by batching and caching instructor queries, reducing redundant calls.
- Adds filtering support to allow dynamic querying of data based on specified criteria.
 */
services
    .AddGraphQLServer()
    .AddQueryType<Query>() // Registers query operations (e.g., fetching courses, students, instructors).
    .AddMutationType<Mutation>() // Registers mutation operations (e.g., adding/updating data).
    .AddSubscriptionType<Subscription>() // Enables real-time updates via subscriptions.
    .AddInMemorySubscriptions() // Uses in-memory event handling for real-time data updates.
    .RegisterDbContextFactory<SchoolDbContext>() // Ensures DbContext instances are properly managed in GraphQL requests.
    .AddDataLoader<InstructorDataLoader>() // Uses a data loader to efficiently batch and cache instructor queries.
    .AddDataLoader<UserDataLoader>() // Uses a data loader to efficiently batch and cache user queries.
    .AddFiltering() // Enables filtering support for GraphQL queries.
    .AddSorting() // Enables sorting support for GraphQL queries.
    .AddProjections() // Enables projecttions support for GraphQL queries.
    .AddAuthorization() // Adds the default authorization support to the schema that uses Microsoft.AspNetCore.Authorization.
    .AddType<CourseType>() // Registers the GraphQL type for Course DTO (used to shape the course response).
    .AddType<InstructorType>() // Registers the GraphQL type for Instructor DTO (used to shape the instructor response).
    .AddTypeExtension<CourseQuery>(); // Registers CourseQuery as a GraphQL type extension to organize related query fields.

// Registers FirebaseApp as a singleton, ensuring a single instance is shared across the application.
// This initializes Firebase using default settings (from appsettings.json or environment variables).
services.AddSingleton(FirebaseApp.Create());

/*
 * Configures Firebase Authentication in the application.
 * This method likely:
 * - Enables JWT-based authentication using Firebase-issued tokens.
 * - Registers authentication handlers for Firebase Auth.
 * - Allows secure API access based on Firebase user authentication.
 */
services.AddFirebaseAuthentication();

/*
 * Defines an "IsAdmin" authorization policy that restricts access to users with a specific email.
 * - Uses Firebase authentication claims to validate the user's identity.
 * - Requires the user to have an EMAIL claim matching "dansari@calrom.com".
 * - This policy can be applied to controllers or actions using [Authorize(Policy = "IsAdmin")].
 */
services.AddAuthorization(o => o.AddPolicy("IsAdmin", p => p.RequireClaim(FirebaseUserClaimType.EMAIL, "dansari@calrom.com")));

// Retrieves the database connection string from the configuration file (appsettings.json or environment variables).
string connectionString = configuration.GetConnectionString("default");

/*
Registers a pooled DbContext factory for efficient database access:
- Uses connection pooling to minimize DbContext creation overhead.
- Ensures each request gets a fresh, independent DbContext instance.
- Improves performance and reduces memory usage by reusing context instances when possible.
- Configures SQLite as the database provider with the specified connection string.
*/
services.AddPooledDbContextFactory<SchoolDbContext>(o => o.UseSqlite(connectionString).LogTo(Console.WriteLine));

services.AddScoped<CoursesRepository>();
services.AddScoped<InstructorsRepository>();

var app = builder.Build();

// Create a new scoped service provider to resolve dependencies.
// This ensures that the DbContextFactory is used within a controlled scope.
using (IServiceScope scope = app.Services.CreateScope())
{
    // Retrieve the IDbContextFactory for SchoolDbContext from the service provider.
    // This factory allows creating new instances of the DbContext as needed.
    IDbContextFactory<SchoolDbContext> contextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<SchoolDbContext>>();

    // Create a new instance of SchoolDbContext from the factory.
    using (SchoolDbContext context = contextFactory.CreateDbContext())
    {
        // Ensures the database is created if it does not already exist.
        context.Database.EnsureCreated();

        // Applies any pending migrations to bring the database schema up to date.
        context.Database.Migrate();
    }
}

app.UseRouting();

app.UseAuthorization();

app.UseWebSockets();

// Configures the application's endpoint routing.
app.UseEndpoints(endpoints =>
{
    // Maps the GraphQL endpoint to handle incoming GraphQL requests.
    _ = endpoints.MapGraphQL().WithOptions(new HotChocolate.AspNetCore.GraphQLServerOptions
    {
        // Allows schema requests, enabling tools like GraphQL Playground and Banana Cake Pop
        // to introspect the schema and provide an interactive UI for querying.
        EnableSchemaRequests = true,

        // Enables the GraphQL development tool (Banana Cake Pop) only in the development environment.
        // This ensures that the tool is accessible for debugging but disabled in production for security.
        Tool =
        {
            Enable = builder.Environment.IsDevelopment()
        }
    });
});

app.Run();
