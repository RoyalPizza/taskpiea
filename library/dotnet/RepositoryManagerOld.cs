using Taskiea.Core.Accounts;
using Taskiea.Core.Connections;

namespace Taskiea.Core;

// this is basically a server locator / manager
public class RepositoryManagerOld
{
    public enum RepositoryType { Manual, Sqlite, HTTP }
    private readonly Dictionary<Type, object> _repositories = new Dictionary<Type, object>();
    private HttpClient? _httpClient;

    public RepositoryManagerOld(RepositoryType repositoryType, BaseConnectionData connectionData)
    {
        if (repositoryType == RepositoryType.Manual && connectionData is SqliteConnectionData sqliteConnectionData)
            return;

        else if (repositoryType == RepositoryType.Sqlite)
        {
            Register(typeof(User), new UserRepositorySqlite());
            Register(typeof(Task), new TaskRepositorySqlite());
        }

        else if (repositoryType == RepositoryType.HTTP && connectionData is HTTPConnectionData httpConnectionData)
        {
            // TODO: Decide if I want to allow the client to make the client instead
            // TODO: Add error checks incase endpoint is invalid?
            _httpClient = new HttpClient() { BaseAddress = new Uri(httpConnectionData.Endpoint) };

            Register(typeof(User), new UserRepositoryHTTP());
            Register(typeof(Task), new TaskRepositoryHTTP());
        }
    }

    public void Register(Type type, object obj)
    {
        _repositories[type] = obj;
    }

    public T1 Get<T, T1>() where T : IEntity where T1 : class
    {
        if (_repositories.TryGetValue(typeof(T), out var repo))
            return (T1)repo;
        throw new InvalidOperationException($"No repository registered for {typeof(T).Name}");
    }

    // A workaroudn so the a server in a server/client model can pass
    // the SQL connection data without recreating all the objects
    public void InitializeAll(BaseConnectionData projectConnectionData)
    {
        foreach (var repo in _repositories.Values)
        {
            if (repo is IRepository<IEntity, BaseConnectionData> entityRepository)
            {
                entityRepository.Initialize(projectConnectionData);
            }
        }
    }
}
