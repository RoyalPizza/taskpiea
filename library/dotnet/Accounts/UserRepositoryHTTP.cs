
namespace Taskiea.Core.Accounts
{
    public class UserRepositoryHTTP : BaseHTTPRepository<User>
    {
        public UserRepositoryHTTP(HttpClient httpClient) : base(httpClient)
        {

        }
    }
}
