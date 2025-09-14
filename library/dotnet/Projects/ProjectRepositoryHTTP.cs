namespace Taskiea.Core.Accounts
{
    public class ProjectRepositoryHTTP : BaseHTTPRepository<User>
    {
        public ProjectRepositoryHTTP(HttpClient httpClient) : base(httpClient)
        {

        }
    }
}
