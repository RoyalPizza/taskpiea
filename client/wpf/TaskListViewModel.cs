using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Data;
using Taskpiea.Core.Results;
using Taskpiea.Core.Tasks;

namespace Taskpiea.WPFClient;

internal class TaskListViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public ICollectionView TasksView { get; }

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

    private readonly ITaskRepository _taskRepository;

    public TaskListViewModel()
    {
        _taskRepository = AppDataCache.shared.RepositoryManager.Get<ITaskRepository>();
        TasksView = CollectionViewSource.GetDefaultView(AppDataCache.shared.Tasks);
        TasksView.Filter = TaskFilter;
    }

    protected void OnPropertyChanged(string name)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    private bool TaskFilter(object obj)
    {
        if (obj is not TaskItem task)
            return false;
        return !ShowCompletedOnly || task.Status == Status.Done;
    }

    public async Task CreateAsync(TaskItem taskItem)
    {
        Debug.WriteLine("TaskListViewModel::CreateAsync");
        ArgumentNullException.ThrowIfNull(taskItem);

        var validateResult = await _taskRepository.ValidateCreateAsync(AppDataCache.ProjectName, taskItem);
        if (validateResult.ResultCode == ResultCode.Success)
        {
            var createResult = await _taskRepository.CreateAsync(AppDataCache.ProjectName, taskItem);
            if (createResult.ResultCode == ResultCode.Success)
            {
                // do nothing because binding handles updates
            }
            else
            {
                // TODO: Log/display error
            }
        }
        else
        {
            // TODO: Log/display error
        }
    }

    public async Task UpdateAsync(TaskItem taskItem)
    {
        Debug.WriteLine("TaskListViewModel::UpdateAsync");
        ArgumentNullException.ThrowIfNull(taskItem);

        var validateResult = await _taskRepository.ValidateUpdateAsync(AppDataCache.ProjectName, taskItem);
        if (validateResult.ResultCode == ResultCode.Success)
        {
            var updateResult = await _taskRepository.UpdateAsync(AppDataCache.ProjectName, taskItem);
            if (updateResult.ResultCode == ResultCode.Success)
            {
                // do nothing because binding handles updates
            }
            else
            {
                // TODO: Log/display error
            }
        }
        else
        {
            // TODO: Log/display error
        }
    }

    public async Task DeleteAsync(TaskItem taskItem)
    {
        Debug.WriteLine("TaskListViewModel::DeleteAsync");
        ArgumentNullException.ThrowIfNull(taskItem);

        var deleteResult = await _taskRepository.DeleteAsync(AppDataCache.ProjectName, taskItem.Id);
        if (deleteResult.ResultCode == ResultCode.Success)
            AppDataCache.shared.Tasks.Remove(taskItem);
        else
        {
            // TODO: Log/display error
        }
    }

    public async Task CreateDummyDataAsync(uint numberOfRecords)
    {
        // TODO: Remove

        for (int i = 0; i < numberOfRecords; i++)
        {
            var taskItem = new TaskItem()
            {
                Name = $"Dummy Task {i}",
                Description = $"Test description {i}",
                Status = Status.Open,
                Assignee = null
            };
            var result = await _taskRepository.CreateAsync(AppDataCache.ProjectName, taskItem);
        }
    }
}
