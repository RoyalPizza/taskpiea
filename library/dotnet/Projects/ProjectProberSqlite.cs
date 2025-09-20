using Taskpiea.Core.Connections;

namespace Taskpiea.Core.Projects;

public class ProjectProberSqlite : IProjectProber
{
    private string _basePath;

    public ProjectProberSqlite()
    {
        // use appdata/roaming as our default path for new projects
        string appDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _basePath = Path.Combine(appDataDirectory, Globals.APP_NAME);
        Directory.CreateDirectory(_basePath);
    }

    public string GetDefaultProjectDirectory()
    {
        return _basePath;
    }

    /// <inheritdoc/>
    public string GetNewDefaultProjectName()
    {
        int counter = 1;
        string projectName = null!;
        string filepath = null!;

        do
        {
            projectName = $"{Globals.NEW_PROJECT_TEMP_NAME} {counter}";
            filepath = Path.Combine(_basePath, $"{projectName}{SqliteConnectionData.PROJECT_EXTENSION}");
            counter++;
        } while (File.Exists(filepath));

        return projectName;
    }

    /// <inheritdoc/>
    public List<string> GetAllProjectNames()
    {
        string[] files = Directory.GetFiles(_basePath, $"*{SqliteConnectionData.PROJECT_EXTENSION}");
        return files.Select(file => Path.GetFileNameWithoutExtension(file)).ToList();
    }

    public string GetProjectFileExtension()
    {
        return SqliteConnectionData.PROJECT_EXTENSION;
    }
}
