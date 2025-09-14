namespace Taskiea.Core;

public interface IRepositoryManager
{
    void Register<T>(IRepository repository) where T : IRepository;
    T Get<T>() where T : IRepository;
}
