using System.Collections.ObjectModel;
using System.ComponentModel;
using Taskpiea.Core;
using Taskpiea.Core.Accounts;
using Taskpiea.Core.Connections;
using Taskpiea.Core.Projects;
using Taskpiea.Core.Results;
using Taskpiea.Core.Tasks;

namespace Taskpiea.WPFClient;

public sealed class AppDataCache : INotifyPropertyChanged
{
    public static AppDataCache shared { get; } = new AppDataCache();
    public static string ProjectName => shared.Project?.Name ?? ""; // just for convinience

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

    public ObservableCollection<TaskItem> Tasks { get; } = new();
    public ObservableCollection<User> Users { get; } = new();
    private ITaskRepository? _taskRepository;
    private IUserRepository? _userRepository;

    private AppDataCache()
    {
        ConnectionCache = new ConnectionCache();
        ProjectProber = new ProjectProberSqlite();
    }

    public async Task OpenProjectAsync<TConnection>(TConnection connectionData) where TConnection : BaseConnectionData
    {
        Project = new Project() { Name = connectionData.ProjectName };

        ConnectionCache.UnregisterAll();
        ConnectionCache.Register(connectionData);
        RepositoryManager = new RepositoryManager(ConnectionCache, connectionData);

        _taskRepository = RepositoryManager.Get<ITaskRepository>();
        _userRepository = RepositoryManager.Get<IUserRepository>();

        await Refresh();
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
        if (Project is null || _taskRepository is null || _userRepository is null)
            return;

        var tasksRequest = await _taskRepository.GetAllAsync(Project.Name, cancellationToken);
        if (tasksRequest.ResultCode == ResultCode.Success)
        {
            Tasks.Clear();
            foreach (var task in tasksRequest.Entities)
                Tasks.Add(task);
        }

        var usersRequest = await _userRepository.GetAllAsync(Project.Name, cancellationToken);
        if (usersRequest.ResultCode == ResultCode.Success)
        {
            Users.Clear();
            foreach (var user in usersRequest.Entities)
                Users.Add(user);
        }
    }

    protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
