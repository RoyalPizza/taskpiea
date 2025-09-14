namespace Taskiea.Core;

// this is basically a server locator / manager
public class RepositoryManager
{
    private readonly Dictionary<Type, IRepository<IEntity>> _repositories = new Dictionary<Type, IRepository<IEntity>>();

    public void Register<T>(IRepository<T> repository) where T : IEntity
    {
        _repositories[typeof(T)] = (IRepository<IEntity>)repository;
    }

    public IRepository<T> Get<T>() where T : IEntity
    {
        if (_repositories.TryGetValue(typeof(T), out var repo))
            return (IRepository<T>)repo;
        throw new InvalidOperationException($"No repository registered for {typeof(T).Name}");
    }

    public void InitializeAll(ProjectConnectionData projectConnectionData)
    {
        foreach (var repo in _repositories.Values)
        {
            repo.Initialize(projectConnectionData);
        }
    }
}
