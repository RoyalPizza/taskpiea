using Taskiea.Core.Connections;

namespace Taskiea.Core;

public interface IRepositoryManager
{
    //public void Register(Type type, object obj);
    public IRepository<IEntity, BaseConnectionData> Get<T>() where T : IEntity;
}
