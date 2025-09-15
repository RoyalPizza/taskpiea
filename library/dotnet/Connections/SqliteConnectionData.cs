namespace Taskiea.Core.Connections;

public class SqliteConnectionData : BaseConnectionData
{
    public string? ProjectPath { get; set; }
    public string ConnectionString { get; set; }

    public SqliteConnectionData(string projectName, string? projectPath = null) : base(projectName)
    {
        ProjectPath = projectPath;

        if (string.IsNullOrEmpty(projectPath))
        {
            ConnectionString = $"Data Source={projectName}.taskp";
        }
        else
        {
            // TODO: validate path (e.g., check if directory exists, sanitize input)
            ConnectionString = $"Data Source={projectPath}/{projectName}.taskp";
        }
    }

    public override void Dispose()
    {
        // nothing to dispose
    }
}
