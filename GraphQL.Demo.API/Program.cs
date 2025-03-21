using GraphQL.Demo.API.Schema.Mutations;
using GraphQL.Demo.API.Schema.Queries;
using GraphQL.Demo.API.Schema.Subscriptions;
using GraphQL.Demo.API.Services;
using GraphQL.Demo.API.Services.Courses;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration; 

services.AddControllers();

services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddSubscriptionType<Subscription>()
    .AddInMemorySubscriptions()
    .RegisterDbContextFactory<SchoolDbContext>(); // This allows GraphQL to resolve SchoolDbContext correctly instead of trying to instantiate it directly.

string connectionString = configuration.GetConnectionString("default");
services.AddPooledDbContextFactory<SchoolDbContext>(o => o.UseSqlite(connectionString));
services.AddScoped<CoursesRepository>();

var app = builder.Build();

using (IServiceScope scope = app.Services.CreateScope())
{
    IDbContextFactory<SchoolDbContext> contextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<SchoolDbContext>>();
    using (SchoolDbContext context = contextFactory.CreateDbContext())
    {
        context.Database.EnsureCreated(); // Ensure DB exists
        context.Database.Migrate(); // Apply Migrations
    }
}

app.UseRouting();

app.UseWebSockets();

app.UseEndpoints(endpoints =>
{
    _ = endpoints.MapGraphQL().WithOptions(new HotChocolate.AspNetCore.GraphQLServerOptions
    {
        EnableSchemaRequests = true,
        Tool =
        {
            Enable = builder.Environment.IsDevelopment()
        }
    });
});

app.Run();
