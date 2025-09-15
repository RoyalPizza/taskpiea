using Taskiea.Core.Accounts;
using Taskiea.Core.Connections;
using Taskiea.Core.Tasks;

namespace Taskiea.Core;

/// <summary>
/// 
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
    private HttpClient? _httpClient;

    public RepositoryManager(BaseConnectionData connectionData)
    {
        if (connectionData is SqliteConnectionData sqliteConnectionData)
        {
            Register<IUserRepository>(new UserRepositorySqlite());
            Register<ITaskRepository>(new TaskRepositorySqlite());
        }

        else if (connectionData is HTTPConnectionData httpConnectionData)
        {
            // TODO: Decide if I want to allow the client to make the client instead
            // TODO: Add error checks incase endpoint is invalid?
            _httpClient = new HttpClient() { BaseAddress = new Uri(httpConnectionData.Endpoint) };

            Register<IUserRepository>(new UserRepositoryHTTP());
            Register<ITaskRepository>(new TaskRepositoryHTTP());
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
