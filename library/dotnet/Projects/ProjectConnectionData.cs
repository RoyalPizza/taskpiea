namespace Taskiea.Core;

public sealed class ProjectConnectionData
{
    public string ProjectName { get; set; } = "";
    public string Projectpath { get; set; } = "";

    public ProjectConnectionData() { }
    public ProjectConnectionData(string projectName) : this(projectName, "") { }
    public ProjectConnectionData(string projectName, string projectPath)
    {
        ArgumentNullException.ThrowIfNull(projectName);
        ArgumentNullException.ThrowIfNull(projectPath);

        ProjectName = projectName;
        Projectpath = projectPath;
    }
}
