namespace GraphQL.Demo.API.Schema.Queries
{
    [InterfaceType("Search")]
    public interface ISearchResultType
    {
        Guid Id { get; }
    }
}
