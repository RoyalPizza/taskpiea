namespace Taskpiea.Core.Projects;

public class Project : IEntity
{
    public string Name { get; init; } = "";
    public DateTimeOffset CreatedOn { get; init; } = DateTimeOffset.UtcNow;

    public uint GetId()
    {
        // so projects will never have a unique "Id". They use GUID.
        // the datalayers for project use the connection data, not this Id.
        // we want CRUD operations, but this one class is unique.
        // Setting to max value so its clear this is not to be used and 
        // not just "not set" from a bad http call or something.
        return uint.MaxValue;
    }
}
