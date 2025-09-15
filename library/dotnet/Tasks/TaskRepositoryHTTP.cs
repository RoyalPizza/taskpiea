using Taskiea.Core.Connections;
using Taskiea.Core.Tasks;

namespace Taskiea.Core.Accounts
{
    public class TaskRepositoryHTTP : BaseHTTPRepository<TaskItem>, ITaskRepository
    {
        public TaskRepositoryHTTP(IConnectionCache connectionCache) : base(connectionCache)
        {
        }
    }
}
