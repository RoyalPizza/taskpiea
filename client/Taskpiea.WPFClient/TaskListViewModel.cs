using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;
using System.Windows.Data;
using Taskpiea.Core.Results;
using Taskpiea.Core.Tasks;

namespace Taskpiea.WPFClient;

public class TaskListViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public ICollectionView TasksView { get; }
    public IAsyncRelayCommand AddTaskCommand { get; }
    public IAsyncRelayCommand DeleteTaskCommand { get; }
    public IRelayCommand ToggleCompletedFilterCommand { get; }
    public IAsyncRelayCommand CreateNewTaskCommand { get; }

    private bool _showCompletedOnly;
    public bool ShowCompletedOnly
    {
        get => _showCompletedOnly;
        set
        {
            // TODO: Decide if we want this check purely because of app refresh, log triggers
            if (_showCompletedOnly != value)
            {
                _showCompletedOnly = value;
                OnPropertyChanged(nameof(ShowCompletedOnly));
                TasksView.Refresh();
            }
        }
    }

    private string _newTaskName = "";
    public string NewTaskName
    {
        get => _newTaskName;
        set
        {
            _newTaskName = value;
            OnPropertyChanged(nameof(NewTaskName));
            CreateNewTaskCommand.NotifyCanExecuteChanged();
        }
    }

    private string _newTaskDescription = "";
    public string NewTaskDescription
    {
        get => _newTaskDescription;
        set
        {
            _newTaskDescription = value;
            OnPropertyChanged(nameof(NewTaskDescription));
        }
    }

    private readonly ITaskRepository _taskRepository;

    public TaskListViewModel()
    {
        _ = LoadAsync();

        // TODO: Later on consider DI but I need to wait until I code more UI first
        _taskRepository = AppDataCache.shared.RepositoryManager.Get<ITaskRepository>();

        TasksView = CollectionViewSource.GetDefaultView(AppDataCache.shared.Tasks);
        TasksView.Filter = TaskFilter;

        AddTaskCommand = new AsyncRelayCommand(CreateAsync);
        DeleteTaskCommand = new AsyncRelayCommand<TaskItem>(DeleteAsync);
        ToggleCompletedFilterCommand = new RelayCommand(() => { ShowCompletedOnly = !ShowCompletedOnly; });
        CreateNewTaskCommand = new AsyncRelayCommand(CreateAsync, NewTaskNameValid);
    }

    protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    private bool NewTaskNameValid() => !string.IsNullOrWhiteSpace(NewTaskName);

    private bool TaskFilter(object obj)
    {
        if (obj is not TaskItem task)
            return false;
        return !ShowCompletedOnly || task.Status == Status.Done;
    }

    public async Task LoadAsync()
    {
        await AppDataCache.shared.Refresh();
    }

    private async Task CreateAsync()
    {
        var newTask = new TaskItem
        {
            Name = NewTaskName,
            Description = NewTaskDescription,
            Status = Status.Open
        };

        var validateResult = await _taskRepository.ValidateCreateAsync(AppDataCache.shared.Project.Name, newTask);
        if (validateResult.ResultCode == ResultCode.Success)
        {
            var createResult = await _taskRepository.CreateAsync(AppDataCache.shared.Project.Name, newTask);
            if (createResult.ResultCode == ResultCode.Success)
            {
                AppDataCache.shared.Tasks.Add(newTask);
                NewTaskName = "";
                NewTaskDescription = "";
            }
        }

        // TODO: Remove
        for (int i = 0; i < 100; i++)
        {
            TaskItem item = new TaskItem() { Name = i.ToString(), Description = $"Test description {i}", Status = Status.Open, Assignee = 0 };
            var result = _taskRepository.CreateAsync(AppDataCache.shared.Project.Name, item);
        }
    }

    private async Task DeleteAsync(TaskItem? task)
    {
        ArgumentNullException.ThrowIfNull(task);

        var deleteResult = await _taskRepository.DeleteAsync(AppDataCache.shared.Project.Name, task.Id);
        if (deleteResult.ResultCode == ResultCode.Success)
            AppDataCache.shared.Tasks.Remove(task);
    }
}
