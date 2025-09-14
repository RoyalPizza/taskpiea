using Taskiea.Core.Accounts;
using Taskiea.Core.Connections;

namespace Taskiea.Core;

// this is basically a server locator / manager
public class RepositoryManager : IRepositoryManager
{
    //private readonly Dictionary<Type, IRepository<IEntity, SqliteConnectionData>> _repositories = new Dictionary<Type, IRepository<IEntity, SqliteConnectionData>>();
    //private readonly Dictionary<Type, IRepository<IEntity, BaseConnectionData>> _repositories = new();
    private readonly Dictionary<Type, IRepository<IEntity, BaseConnectionData>> _repositories = new();
    private HttpClient? _httpClient;

    public RepositoryManager(BaseConnectionData connectionData)
    {
        if (connectionData is SqliteConnectionData sqliteConnectionData)
        {
            Register(typeof(User), new UserRepositorySqlite());
            Register(typeof(Task), new TaskRepositorySqlite());
        }

        else if (connectionData is HTTPConnectionData httpConnectionData)
        {
            // TODO: Decide if I want to allow the client to make the client instead
            // TODO: Add error checks incase endpoint is invalid?
            _httpClient = new HttpClient() { BaseAddress = new Uri(httpConnectionData.Endpoint) };

            Register(typeof(User), new UserRepositoryHTTP());
            Register(typeof(Task), new TaskRepositoryHTTP());
        }
    }

    public void Register(Type type, IRepository<IEntity, BaseConnectionData> repository)
    {
        //_repositories[type] = repository;
    }
    
    public void Register<T1, T2>(Type type, IRepository<T1, T2> obj) where T1 : IEntity where T2 : BaseConnectionData
    {
        if (obj is IRepository<IEntity, BaseConnectionData>)
        {
            // no
        }
        if (obj is IRepository<IEntity, SqliteConnectionData>)
        {
            // no
        }
        if (obj is IRepository<User, SqliteConnectionData> newObj)
        {
            // yes

            if (newObj is IRepository<IEntity, SqliteConnectionData>)
            {

            }
        }

        //_repositories[type] = obj;

        //var z = (IRepository<IEntity, SqliteConnectionData>)obj;
        //_repositories[type] = z;
    }

    //public T1 Get<T, T1>() where T : IEntity where T1 : class
    //{
    //    if (_repositories.TryGetValue(typeof(T), out var repo))
    //        return (T1)repo;
    //    throw new InvalidOperationException($"No repository registered for {typeof(T).Name}");
    //}

    public IRepository<IEntity, BaseConnectionData> Get<T>() where T : IEntity
    {
        if (_repositories.TryGetValue(typeof(T), out var repo))
            return (IRepository<IEntity, BaseConnectionData>)repo;
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
