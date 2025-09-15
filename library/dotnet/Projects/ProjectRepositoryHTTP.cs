using Taskiea.Core.Connections;

namespace Taskiea.Core.Accounts
{
    public class ProjectRepositoryHTTP : BaseHTTPRepository<User>
    {
        public ProjectRepositoryHTTP(IConnectionCache connectionCache) : base(connectionCache)
        {
        }
    }
}
