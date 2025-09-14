namespace Taskiea.Core;

public interface IRepositoryManager
{
    void Register(Type type, IRepository repository);
    T Get<T>() where T : IRepository;
}
