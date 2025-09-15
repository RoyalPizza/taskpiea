using Taskpiea.Core.Connections;
using Taskpiea.Core.Tasks;

namespace Taskpiea.Core.Accounts
{
    public class TaskRepositoryHTTP : BaseHTTPRepository<TaskItem>, ITaskRepository
    {
        public TaskRepositoryHTTP(IConnectionCache connectionCache) : base(connectionCache)
        {
        }
    }
}
