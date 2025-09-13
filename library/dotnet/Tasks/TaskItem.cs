namespace Taskiea.Core.Tasks;

public sealed class TaskItem : IDataObject
{
    public uint Id { get; set; }

    public string Name { get; set; } = "";

    public string Description { get; set; } = "";

    public Status Status { get; set; }
}
