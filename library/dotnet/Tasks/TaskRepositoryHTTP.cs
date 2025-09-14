namespace Taskiea.Core.Accounts
{
    public class TaskRepositoryHTTP : BaseHTTPRepository<User>
    {
        public TaskRepositoryHTTP(HttpClient httpClient) : base(httpClient)
        {

        }
    }
}
