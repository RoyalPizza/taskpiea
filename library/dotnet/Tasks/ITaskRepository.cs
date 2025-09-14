using Taskiea.Core.Connections;

namespace Taskiea.Core.Tasks;

public interface ITaskRepository<T> : IRepository<TaskItem, T> where T : BaseConnectionData
{
}
