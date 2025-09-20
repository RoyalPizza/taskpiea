using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Data;
using Taskpiea.Core.Accounts;
using Taskpiea.Core.Results;

namespace Taskpiea.WPFClient;

internal class UserListViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public ICollectionView UsersView { get; }

    private readonly IUserRepository _userRepository;

    public UserListViewModel()
    {
        _userRepository = AppDataCache.shared.RepositoryManager.Get<IUserRepository>();
        UsersView = CollectionViewSource.GetDefaultView(AppDataCache.shared.Users);
    }

    protected void OnPropertyChanged(string name)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public async Task<CRUDResult<User>> CreateAsync(User user)
    {
        Debug.WriteLine("UserListViewModel::CreateAsync");
        ArgumentNullException.ThrowIfNull(user);

        var validateResult = await _userRepository.ValidateCreateAsync(AppDataCache.ProjectName, user);
        if (validateResult.ResultCode == ResultCode.Success)
        {
            var createResult = await _userRepository.CreateAsync(AppDataCache.ProjectName, user);
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

        return null;

        //await CreateDummyDataAsync(50);
    }

    public async Task UpdateAsync(User user)
    {
        Debug.WriteLine("UserListViewModel::UpdateAsync");
        ArgumentNullException.ThrowIfNull(user);

        var validateResult = await _userRepository.ValidateUpdateAsync(AppDataCache.ProjectName, user);
        if (validateResult.ResultCode == ResultCode.Success)
        {
            var updateResult = await _userRepository.UpdateAsync(AppDataCache.ProjectName, user);
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

    public async Task DeleteAsync(User user)
    {
        Debug.WriteLine("TaskListViewModel::DeleteAsync");
        ArgumentNullException.ThrowIfNull(user);

        var deleteResult = await _userRepository.DeleteAsync(AppDataCache.ProjectName, user.Id);
        if (deleteResult.ResultCode == ResultCode.Success)
            AppDataCache.shared.Users.Remove(user);
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
            var user = new User() { Name = $"Dummy User {i}" };
            var result = await _userRepository.CreateAsync(AppDataCache.ProjectName, user);
        }
    }
}