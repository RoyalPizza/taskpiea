using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Taskpiea.Core.Tasks;

public sealed class TaskItem : IEntity //, IEditableObject
{
    public uint Id { get; set; }

    public string Name { get; set; } = "";

    public string Description { get; set; } = "";

    public Status Status { get; set; }

    public int? Assignee { get; set; } // TODO: implement

    [JsonIgnore]
    private TaskItem? _backupEditData;
    [JsonIgnore]
    bool inTxn;

    public void BeginEdit()
    {
        if (!inTxn)
        {
            _backupEditData = new TaskItem()
            {
                Id = Id,
                Name = Name,
                Description = Description,
                Status = Status,
                Assignee = Assignee,
            };
            inTxn = true;
        }
    }

    public void CancelEdit()
    {
        if (inTxn)
        {
            ArgumentNullException.ThrowIfNull(_backupEditData);

            Id = _backupEditData.Id;
            Name = _backupEditData.Name;
            Description = _backupEditData.Description;
            Status = _backupEditData.Status;
            Assignee = _backupEditData.Assignee;
            inTxn = false;
            _backupEditData = null;
        }
    }

    public void EndEdit()
    {
        if (inTxn)
        {
            _backupEditData = null;
            inTxn = false;
        }
    }

    public uint GetId() => Id;
}
