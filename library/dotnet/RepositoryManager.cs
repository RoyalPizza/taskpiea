using Taskiea.Core.Accounts;
using Taskiea.Core.Connections;
using Taskiea.Core.Tasks;

namespace Taskiea.Core;

/// <summary>
/// TODO
/// </summary>
/// <remarks>
/// By nature, only one connection type is supported at a time.
/// Either HTTP, Sqlite, etc..
/// </remarks>
public class RepositoryManager : IRepositoryManager
{
    // TODO: Right now the cache assumes the type is the interface like IUserRepository
    //       This is is not type safe currently. Because its just type, so the developer
    //       may try to register / get the wrong type.
    //       Is there a way to store type but only allow type <IRepository>
    private readonly Dictionary<Type, IRepository> _repositories = new();
    private IConnectionCache _connectionCache;

    public RepositoryManager(IConnectionCache connectionCache, BaseConnectionData connectionData)
    {
        ArgumentNullException.ThrowIfNull(connectionCache);

        _connectionCache = connectionCache;

        if (connectionData is SqliteConnectionData sqliteConnectionData)
        {
            Register<IUserRepository>(new UserRepositorySqlite(connectionCache));
            Register<ITaskRepository>(new TaskRepositorySqlite(connectionCache));
        }

        else if (connectionData is HTTPConnectionData httpConnectionData)
        {
            Register<IUserRepository>(new UserRepositoryHTTP(connectionCache));
            Register<ITaskRepository>(new TaskRepositoryHTTP(connectionCache));
        }

        InitializeAll(connectionData);
    }

    public void Register<T>(IRepository repository) where T : IRepository
    {
        _repositories[typeof(T)] = repository;
    }

    public void Unregister<T>() where T : IRepository
    {
        _repositories.Remove(typeof(T));
    }

    public T Get<T>() where T : IRepository
    {
        if (_repositories.TryGetValue(typeof(T), out var repo))
            return (T)repo;

        throw new InvalidOperationException($"No repository registered for {typeof(T).Name}");
    }

    /// <summary>
    /// A helper function for scenarios where we want to reinitialize connections,
    /// like when standalone apps switch projects or when a server receives a request
    /// for another project
    /// </summary>
    /// <param name="connectionData"></param>
    public void InitializeAll(BaseConnectionData connectionData)
    {
        foreach (var repo in _repositories.Values)
            repo.Initialize(connectionData.ProjectName);
    }
}
