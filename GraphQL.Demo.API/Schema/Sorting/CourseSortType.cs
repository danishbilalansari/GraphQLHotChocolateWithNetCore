using GraphQL.Demo.API.Schema.Queries;

using HotChocolate.Data.Sorting;

/// <summary>
/// Defines sorting rules for the CourseType GraphQL queries.
/// </summary>
/// <remarks>
/// - Extends HotChocolate's 'SortInputType<CourseType>' to enable sorting capabilities in GraphQL queries.
/// - Ignores sorting by 'Id' and 'InstructorId' to prevent unnecessary sorting on those fields.
/// - Renames the sorting field for 'Name' to '"CourseName"' to provide a more user-friendly sorting option.
/// - Helps optimize sorting operations by explicitly defining which fields can be sorted, reducing potential performance overhead.
/// </remarks>
public class CourseSortType : SortInputType<CourseType>
{
    /// <summary>
    /// Configures the sorting options for CourseType.
    /// </summary>
    /// <param name="descriptor">The sort input type descriptor.</param>
    protected override void Configure(ISortInputTypeDescriptor<CourseType> descriptor)
    {
        descriptor.Ignore(c => c.Id); // Excludes sorting by Id to avoid unnecessary sorting operations.
        descriptor.Ignore(c => c.InstructorId); // Excludes sorting by InstructorId as it may not be relevant.
        descriptor.Field(c => c.Name).Name("CourseName"); // Allows sorting by Name but exposes it as "CourseName".

        base.Configure(descriptor);
    }
}
