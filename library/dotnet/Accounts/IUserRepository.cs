using Taskiea.Core.Connections;

namespace Taskiea.Core.Accounts;

public interface IUserRepository<T> : IRepository<User, T> where T : BaseConnectionData
{
}
