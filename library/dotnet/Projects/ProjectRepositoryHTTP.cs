using Taskpiea.Core.Connections;

namespace Taskpiea.Core.Accounts
{
    public class ProjectRepositoryHTTP : BaseHTTPRepository<User>
    {
        public ProjectRepositoryHTTP(IConnectionCache connectionCache) : base(connectionCache)
        {
        }
    }
}
