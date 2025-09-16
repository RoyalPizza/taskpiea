namespace Taskpiea.Core.Connections;

public class SqliteConnectionData : BaseConnectionData
{
    public string? ProjectDirectory { get; set; }
    public string ConnectionString { get; set; }

    public const string PROJECT_EXTENSION = ".taskp";

    public SqliteConnectionData(string projectName, string? projectPath = null) : base(projectName)
    {
        ProjectDirectory = projectPath;
        string filePath = GetFilePath();
        ConnectionString = $"Data Source={filePath}";
    }

    public string GetFilePath()
    {
        if (string.IsNullOrEmpty(ProjectDirectory))
            return $"{ProjectName}{PROJECT_EXTENSION}";
        else
            return $"{ProjectDirectory}/{ProjectName}{PROJECT_EXTENSION}";
    }

    public override void Dispose()
    {
        // nothing to dispose
    }
}
