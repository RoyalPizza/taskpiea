namespace Taskpiea.Core.Accounts;

public sealed class User : IEntity
{
    public uint Id { get; set; }

    public string Name { get; set; } = "";

    public uint GetId() => Id;
}
