namespace Taskiea.Core.Tasks;

public sealed class TaskItem : IEntity
{
    public uint Id { get; set; }

    public string Name { get; set; } = "";

    public string Description { get; set; } = "";

    public Status Status { get; set; }

    public int Assignee { get; set; } // TODO: implement

    public uint GetId() => Id;
}
