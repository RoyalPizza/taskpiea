using System.Collections.Concurrent;

namespace Taskiea.Core.Connections;

public static class ConnectionCache
{
    private static readonly ConcurrentDictionary<string, BaseConnectionData> _connectionStringCache = new();

    public static T GetConnectionData<T>(string projectName) where T : BaseConnectionData
    {
        if (_connectionStringCache.TryGetValue(projectName, out var cachedConnectionData))
            return (T)cachedConnectionData;

        // TODO: Maybe put this in a register/unregister system?
        if (typeof(T) == typeof(SqliteConnectionData))
        {
            _connectionStringCache[projectName] = new SqliteConnectionData(projectName, null);
            return (T)_connectionStringCache[projectName];
        }

        throw new NotSupportedException();
    }
}
