namespace Taskpiea.Core.Projects;

public interface IProjectProber
{
    public string GetDefaultProjectDirectory();

    /// <summary>
    /// Checks a valid project name for creating a new project.
    /// </summary>
    /// <returns>Returns a string of the project name.</returns>
    public string GetNewDefaultProjectName();

    /// <summary>
    /// Gets projects names from the default project directory.
    /// </summary>
    /// <returns></returns>
    public List<string> GetAllProjectNames();

    public string GetProjectFileExtension();
}
