using System.Collections.ObjectModel;
using Taskiea.Core;
using Taskiea.Core.Connections;
using Taskiea.Core.Projects;
using Taskiea.Core.Tasks;

namespace Taskiea.WPFClient;

public sealed class AppDataCache
{
    public static AppDataCache shared { get; } = new AppDataCache();

    public IRepositoryManager RepositoryManager { get; private set; }
    public Project Project { get; private set; }

    public ObservableCollection<TaskItem> Tasks { get; } = new ObservableCollection<TaskItem>();
    private ITaskRepository _taskRepository;

    private AppDataCache()
    {
        // This should not be done until a project is created or opened, not here
        Project = new Project() { Name = "Test Project" };
        SqliteConnectionData connectionData = new SqliteConnectionData(Project.Name, null);
        RepositoryManager = new RepositoryManager(connectionData);
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
