using System.Collections.Concurrent;

namespace Taskpiea.Core.Connections;

/// <summary>
/// A class to hold connections that are used by repositories.
/// Register a connection to keep it around as long as needed.
/// If a connection is no longer needed, unregister it.
/// Connections are unique based on proejct name.
/// </summary>
public class ConnectionCache : IConnectionCache
{
    private readonly ConcurrentDictionary<string, BaseConnectionData> _connectionCache = new();

    public void Register<TConnection>(TConnection connection) where TConnection : BaseConnectionData
    {
        ArgumentNullException.ThrowIfNull(connection);
        if (_connectionCache.ContainsKey(connection.ProjectName))
            throw new Exception("Connection already exists for that project name.");

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
            Unregister(connectionData.ProjectName);
    }

    public T? GetConnectionData<T>(string projectName) where T : BaseConnectionData
    {
        if (_connectionCache.TryGetValue(projectName, out var cachedConnectionData))
            return (T)cachedConnectionData;

        return null;
    }

    public void Dispose()
    {
        UnregisterAll();
    }
}
