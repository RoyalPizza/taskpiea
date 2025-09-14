using System.Collections.Concurrent;

namespace Taskiea.Core;

/// <summary>
/// A static cache of connections used by the repositories.
/// </summary>
/// <remarks>
/// This is designed to store kvp with the key being the project name
/// and the value being a the "connection string". Right now the only backedn is Sqlite.
/// If I decide to add support for alternative backends, then this class will need to change.
/// 
/// This class is static and not injected into repositories as of right now. I may consider
/// injecting it
/// </remarks>
internal static class RepositoryCache
{
    // Key: project name -- Value: sqlite connection string
    private static readonly ConcurrentDictionary<string, string> _connectionStringCache = new ConcurrentDictionary<string, string>();

    internal static string GetConnectionString(ProjectConnectionData projectConnectionData)
    {
        if (_connectionStringCache.TryGetValue(projectConnectionData.ProjectName, out var cachedConnectionString))
            return cachedConnectionString;

        string connectionString;
        if (string.IsNullOrEmpty(projectConnectionData.Projectpath))
            connectionString = $"Data Source={projectConnectionData.ProjectName}.taskp";
        else
        {
            // TODO: validate path (e.g., check if directory exists, sanitize input)
            connectionString = $"Data Source={projectConnectionData.Projectpath}/{projectConnectionData.ProjectName}.taskp";
        }
        _connectionStringCache.TryAdd(projectConnectionData.ProjectName, connectionString);

        //

        return connectionString;
    }
}
