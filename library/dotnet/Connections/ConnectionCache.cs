using System.Collections.Concurrent;

namespace Taskiea.Core.Connections;

public class ConnectionCache : IConnectionCache, IDisposable
{
    private readonly ConcurrentDictionary<string, BaseConnectionData> _connectionCache = new();

    public void Register<TConnection>(TConnection connection) where TConnection : BaseConnectionData
    {
        ArgumentNullException.ThrowIfNull(connection);
        _connectionCache[connection.ProjectName] = connection;
    }

    public void Unregister(string projectName)
    {
        _ = _connectionCache.TryRemove(projectName, out var cachedConnectionData);
        cachedConnectionData?.Dispose();
    }

    public void UnregisterAll()
    {
        foreach (var connectionData in _connectionCache.Values)
            connectionData?.Dispose();
    }

    public T? GetConnectionData<T>(string projectName) where T : BaseConnectionData
    {
        if (_connectionCache.TryGetValue(projectName, out var cachedConnectionData))
            return (T)cachedConnectionData;

        // TODO: Maybe put this in a register/unregister system?
        if (typeof(T) == typeof(SqliteConnectionData))
        {
            _connectionCache[projectName] = new SqliteConnectionData(projectName, null);
            return (T)_connectionCache[projectName];
        }
        else if (typeof(T) == typeof(HTTPConnectionData))
        {
            // TODO: how do we handle requesting an HTTP connection that did not exist
            return default;
        }

        throw new NotSupportedException();
    }

    public void Dispose()
    {
        UnregisterAll();
    }
}
