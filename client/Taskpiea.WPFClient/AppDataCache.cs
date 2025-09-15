using System.Collections.ObjectModel;
using Taskpiea.Core;
using Taskpiea.Core.Connections;
using Taskpiea.Core.Projects;
using Taskpiea.Core.Tasks;

namespace Taskpiea.WPFClient;

public sealed class AppDataCache
{
    public static AppDataCache shared { get; } = new AppDataCache();

    public IConnectionCache ConnectionCache { get; init; }
    public IRepositoryManager RepositoryManager { get; private set; }
    public Project Project { get; private set; }

    public ObservableCollection<TaskItem> Tasks { get; } = new ObservableCollection<TaskItem>();
    private ITaskRepository _taskRepository;

    private AppDataCache()
    {
        ConnectionCache = new ConnectionCache();

        // The following code is what needs to be done with a project is created/opened
        // 1) create the connection data based on whether the project is standalone or client/server
        // 2) register the connection data with the connection cache
        // 3) create a repository manager to initialize repositories using that connection data
        // NOTE: It is up to the dev to unregister connections they no longer need so they are disposed and such.
        Project = new Project() { Name = "Test Project" };
        SqliteConnectionData connectionData = new SqliteConnectionData(Project.Name);
        ConnectionCache.Register(connectionData);
        RepositoryManager = new RepositoryManager(ConnectionCache, connectionData);

        _taskRepository = RepositoryManager.Get<ITaskRepository>();
    }

    public async Task Refresh(CancellationToken cancellationToken = default)
    {
        await _taskRepository.GetAllAsync(Project.Name, cancellationToken);

        var result = await _taskRepository.GetAllAsync(Project.Name, cancellationToken);
        if (result.ResultCode == Core.Results.ResultCode.Success)
        {
            Tasks.Clear();
            foreach (var task in result.DataObjects)
                Tasks.Add(task);
        }
    }
}
