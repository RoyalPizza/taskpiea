namespace Taskiea.Core.Connections;

public abstract class BaseConnectionData : IDisposable
{
    public string ProjectName { get; set; }

    public BaseConnectionData(string projectName)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(projectName);
        ProjectName = projectName;
    }

    public abstract void Dispose();
}
