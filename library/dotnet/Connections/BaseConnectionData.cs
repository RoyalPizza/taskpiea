namespace Taskiea.Core.Connections;

public class BaseConnectionData
{
    public string ProjectName { get; set; }

    public BaseConnectionData(string projectName)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(projectName);
        ProjectName = projectName;
    }
}
