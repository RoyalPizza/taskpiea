using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;
using System.Windows.Data;
using Taskpiea.Core.Accounts;
using Taskpiea.Core.Results;

namespace Taskpiea.WPFClient;

internal class UserListViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public ICollectionView UsersView { get; }
    public IAsyncRelayCommand CreateNewUserCommand { get; }
    public IAsyncRelayCommand DeleteUserCommand { get; }

    private string _newUserName = "";
    public string NewUserName
    {
        get => _newUserName;
        set
        {
            _newUserName = value;
            OnPropertyChanged(nameof(NewUserName));
            CreateNewUserCommand.NotifyCanExecuteChanged();
        }
    }

    private readonly IUserRepository _userRepository;

    public UserListViewModel()
    {
        _userRepository = AppDataCache.shared.RepositoryManager.Get<IUserRepository>();

        UsersView = CollectionViewSource.GetDefaultView(AppDataCache.shared.Users);
        CreateNewUserCommand = new AsyncRelayCommand(CreateAsync, NewUserNameValid);
        DeleteUserCommand = new AsyncRelayCommand<User>(DeleteAsync);
    }

    protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    private bool NewUserNameValid() => !string.IsNullOrWhiteSpace(NewUserName);

    private async Task LoadAsync()
    {
        await AppDataCache.shared.Refresh();
    }

    private async Task CreateAsync()
    {
        var newUser = new User()
        {
            Name = NewUserName
        };

        var validateResult = await _userRepository.ValidateCreateAsync(AppDataCache.shared.Project.Name, newUser);
        if (validateResult.ResultCode == ResultCode.Success)
        {
            var createResult = await _userRepository.CreateAsync(AppDataCache.shared.Project.Name, newUser);
            if (createResult.ResultCode == ResultCode.Success)
            {
                AppDataCache.shared.Users.Add(newUser);
                NewUserName = "";
            }
        }
    }

    private async Task DeleteAsync(User? user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var deleteResult = await _userRepository.DeleteAsync(AppDataCache.shared.Project.Name, user.Id);
        if (deleteResult.ResultCode == ResultCode.Success)
            AppDataCache.shared.Users.Remove(user);
    }
}