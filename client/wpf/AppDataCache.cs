using System.Collections.ObjectModel;
using System.ComponentModel;
using Taskpiea.Core;
using Taskpiea.Core.Connections;
using Taskpiea.Core.Projects;
using Taskpiea.Core.Tasks;

namespace Taskpiea.WPFClient;

public sealed class AppDataCache : INotifyPropertyChanged
{
    public static AppDataCache shared { get; } = new AppDataCache();

    public IProjectProber ProjectProber { get; init; }
    public IConnectionCache ConnectionCache { get; init; }
    public IRepositoryManager? RepositoryManager { get; private set; }

    private Project? _project;
    public Project? Project
    {
        get => _project;
        private set
        {
            _project = value;
            OnPropertyChanged(nameof(Project));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<TaskItem> Tasks { get; } = new ObservableCollection<TaskItem>();
    private ITaskRepository? _taskRepository;

    private AppDataCache()
    {
        ConnectionCache = new ConnectionCache();
        ProjectProber = new ProjectProberSqlite();
    }

    public void OpenProject<TConnection>(TConnection connectionData) where TConnection : BaseConnectionData
    {
        Project = new Project() { Name = connectionData.ProjectName };

        ConnectionCache.UnregisterAll();
        ConnectionCache.Register(connectionData);
        RepositoryManager = new RepositoryManager(ConnectionCache, connectionData);

        _taskRepository = RepositoryManager.Get<ITaskRepository>();
    }

    public void CloseProject()
    {
        Project = null;
        ConnectionCache.UnregisterAll();
        RepositoryManager = null;

        _taskRepository = null;
    }

    public async Task Refresh(CancellationToken cancellationToken = default)
    {
        if (_taskRepository is null || Project is null)
            return;

        await _taskRepository.GetAllAsync(Project.Name, cancellationToken);

        var result = await _taskRepository.GetAllAsync(Project.Name, cancellationToken);
        if (result.ResultCode == Core.Results.ResultCode.Success)
        {
            Tasks.Clear();
            foreach (var task in result.DataObjects)
                Tasks.Add(task);
        }
    }

    protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
