using Taskiea.Core.Connections;

namespace Taskiea.Core.Projects;

public interface IProjectRepository<T> : IRepository<Project, T> where T : BaseConnectionData
{
}
