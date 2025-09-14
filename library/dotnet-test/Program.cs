using Taskiea.Core;
using Taskiea.Core.Accounts;
using Taskiea.Core.Projects;
using Taskiea.Core.Tasks;

namespace dotnet_test
{
    internal class Program
    {
        static CancellationTokenSource _tokenSource = new CancellationTokenSource();

        static ICRUDDataLayer<User> _userDataLayer;
        static ICRUDDataLayer<TaskItem> _taskDataLayer;
        static ProjectConnectionData _projectConnectionData;

        static void Main(string[] args)
        {
            Project project = new Project("TestProject");
            _projectConnectionData = new ProjectConnectionData(project.Name);

            // The app decides if it wants HTTP or Storage at this time
            _userDataLayer = new UserStorageDataLayer();
            _taskDataLayer = new TaskStorageDataLayer();

            // These need to be called
            //_userDataLayer.Initialize(projectConnectionData);
            //_taskDataLayer.Initialize(projectConnectionData);

            User user = new User();
            user.Name = "Royal Pizza";

            TaskItem task = new TaskItem();
            task.Name = "Task 1";
            task.Status = Status.Open;
            task.Description = "";

            var userValidateResult = _userDataLayer.ValidateCreateAsync(_projectConnectionData, user, _tokenSource.Token).Result;
            var userCreateResult = _userDataLayer.CreateAsync(_projectConnectionData, user, _tokenSource.Token).Result;

            var taskValidateResult = _taskDataLayer.ValidateCreateAsync(_projectConnectionData, task, _tokenSource.Token).Result;
            var taskCreateResult = _taskDataLayer.CreateAsync(_projectConnectionData, task, _tokenSource.Token).Result;
        }
    }
}