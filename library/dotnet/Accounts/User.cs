namespace Taskiea.Core.Accounts;

public sealed class User : IDataObject
{
    public uint Id { get; set; }

    public string Name { get; set; } = "";
}
