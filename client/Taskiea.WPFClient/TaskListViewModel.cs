using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using Taskiea.Core.Results;
using Taskiea.Core.Tasks;

namespace Taskiea.WPFClient;

public class TaskListViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private string _newTaskName = "";
    public string NewTaskName
    {
        get => _newTaskName;
        set { _newTaskName = value; OnPropertyChanged(nameof(NewTaskName)); }
    }

    private string _newTaskDescription = "";
    public string NewTaskDescription
    {
        get => _newTaskDescription;
        set { _newTaskDescription = value; OnPropertyChanged(nameof(NewTaskDescription)); }
    }

    public ICollectionView TasksView { get; }
    public ICommand AddTaskCommand { get; }
    public ICommand DeleteTaskCommand { get; }
    public ICommand ToggleCompletedFilterCommand { get; }
    public ICommand CommitNewTaskCommand { get; }

    private bool _showCompletedOnly;
    public bool ShowCompletedOnly
    {
        get => _showCompletedOnly;
        set
        {
            if (_showCompletedOnly != value)
            {
                _showCompletedOnly = value;
                OnPropertyChanged(nameof(ShowCompletedOnly));
                TasksView.Refresh();
            }
        }
    }

    private readonly ITaskRepository _taskRepository;
    private CancellationTokenSource _cts;

    private CancellationToken GetNewToken()
    {
        // Not sure if I like this. Perhaps my library should support null tokens so
        // I only code it in the UI if it actually uses it. Here I am just making tokens
        // to satisfy the API
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        return _cts.Token;
    }

    protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public TaskListViewModel()
    {
        _ = LoadAsync();

        // TODO: Later on consider DI but I need to wait until I code more UI first
        _taskRepository = AppDataCache.shared.RepositoryManager.Get<ITaskRepository>();

        TasksView = CollectionViewSource.GetDefaultView(AppDataCache.shared.Tasks);
        TasksView.Filter = TaskFilter;

        // TODO: Change this to work with async relay commands
        // Or just get rid of this? I am still not sure why this is needed. 
        // Need to build more UI to learn? Because I thought my WPF UserControl
        // would just call the functions directly? Not sure.
        AddTaskCommand = new RelayCommand(_ => CreateAsync());
        DeleteTaskCommand = new RelayCommand(task => DeleteAsync(task as TaskItem));
        ToggleCompletedFilterCommand = new RelayCommand(_ =>
        {
            ShowCompletedOnly = !ShowCompletedOnly;
        });
        CommitNewTaskCommand = new RelayCommand(_ => _ = CreateAsync(), _ => !string.IsNullOrWhiteSpace(NewTaskName));
    }

    private bool TaskFilter(object obj)
    {
        if (obj is not TaskItem task)
            return false;
        return !ShowCompletedOnly || task.Status == Status.Done;
    }

    public async Task LoadAsync()
    {
        await AppDataCache.shared.Refresh(GetNewToken());
    }

    private async Task CreateAsync()
    {
        var newTask = new TaskItem
        {
            Name = NewTaskName,
            Description = NewTaskDescription,
            Status = Status.Open
        };

        var validateResult = await _taskRepository.ValidateCreateAsync(AppDataCache.shared.Project.Name, newTask, GetNewToken());
        if (validateResult.ResultCode == Core.Results.ResultCode.Success)
        {
            var createResult = await _taskRepository.CreateAsync(AppDataCache.shared.Project.Name, newTask, GetNewToken());
            if (createResult.ResultCode == ResultCode.Success)
            {
                AppDataCache.shared.Tasks.Add(newTask);
                NewTaskName = "";
                NewTaskDescription = "";
            }
        }

        for (int i = 0; i < 100; i++)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            TaskItem item = new TaskItem() { Name = i.ToString(), Description = $"Test description {i}", Status = Status.Open, Assignee = 0 };
            var result = _taskRepository.CreateAsync(AppDataCache.shared.Project.Name, item, cts.Token);
            cts.Dispose();
        }
    }

    private async Task DeleteAsync(TaskItem? task)
    {
        if (task is null)
            return;

        var deleteResult = await _taskRepository.DeleteAsync(AppDataCache.shared.Project.Name, task.Id, GetNewToken());
        if (deleteResult.ResultCode == ResultCode.Success)
            AppDataCache.shared.Tasks.Remove(task);
    }
}
