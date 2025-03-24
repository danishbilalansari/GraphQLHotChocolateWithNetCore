using GraphQL.Demo.API.Schema.Queries;
using HotChocolate.Data.Filters;

namespace GraphQL.Demo.API.Schema.Filters
{
    /// <summary>
    /// Defines filtering rules for the CourseType GraphQL queries.
    /// </summary>
    /// <remarks>
    /// - Extends HotChocolate's 'FilterInputType<CourseType>' to enable dynamic filtering in GraphQL queries.
    /// - Customizes the filter by ignoring the 'Students' property, preventing it from being filterable.
    /// - Helps optimize database queries by allowing only relevant fields to be used in filtering.
    /// </remarks>
    public class CourseFilterType : FilterInputType<CourseType>
    {
        /// <summary>
        /// Configures the filtering options for CourseType.
        /// </summary>
        /// <param name="descriptor">The filter input type descriptor.</param>
        protected override void Configure(IFilterInputTypeDescriptor<CourseType> descriptor)
        {
            descriptor.Ignore(c => c.Students); // Excludes the Students field from filtering.

            base.Configure(descriptor);
        }
    }
}
