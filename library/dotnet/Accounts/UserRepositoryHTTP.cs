using Taskpiea.Core.Connections;

namespace Taskpiea.Core.Accounts
{
    public class UserRepositoryHTTP : BaseHTTPRepository<User>, IUserRepository
    {
        public UserRepositoryHTTP(IConnectionCache connectionCache) : base(connectionCache)
        {
        }
    }
}
