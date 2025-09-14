using Taskiea.Core;
using Taskiea.Core.Accounts;
using Taskiea.Core.Connections;
using Taskiea.Core.Projects;
using Taskiea.Core.Tasks;

namespace dotnet_test
{
    internal static class Program
    {
        static CancellationTokenSource _tokenSource = new CancellationTokenSource();
        static BaseConnectionData _connectionData = null!;
        static IRepositoryManager _repositoryManager = null!;

        static void Main(string[] args)
        {
            // this mimics me opening a project
            Project project = new Project() { Name = "Test Project" };
            _connectionData = new SqliteConnectionData(project.Name, null);

            // we make a repository manager after opening a project because it defines the connection path
            // this is ideal for standalone. In a server/client model where a website wants to display 
            // all projects that can be opened, that would have to be done manually. Like most logins are
            // anyways. Changing projects means recreating all these, as its possible its a different server
            // or a different db.
            _repositoryManager = new RepositoryManager(_connectionData);

            // navigated to user page
            {
                // a ui page would cache a reference to the repository
                var userRepository = _repositoryManager.Get<User>();

                User user = new User();
                user.Name = "Royal Pizza";

                // validate is called inside create, but you can call it manually for extra checks
                var userValidateResult = userRepository.ValidateCreateAsync(_connectionData, user, _tokenSource.Token).Result;
                var userCreateResult = userRepository.CreateAsync(_connectionData, user, _tokenSource.Token).Result;
            }

            // navigated to task page
            {
                // a ui page would cache a reference to the repository
                var taskRepository = _repositoryManager.Get<TaskItem>();

                TaskItem task = new TaskItem();
                task.Name = "Task 1";
                task.Status = Status.Open;
                task.Description = "";

                // validate is called inside create, but you can call it manually for extra checks
                var taskValidateResult = taskRepository.ValidateCreateAsync(_connectionData, task, _tokenSource.Token).Result;
                var taskCreateResult = taskRepository.CreateAsync(_connectionData, task, _tokenSource.Token).Result;
            }
        }
    }
}