using Taskiea.Core.Connections;

namespace Taskiea.Core.Accounts
{
    public class UserRepositoryHTTP : BaseHTTPRepository<User>, IUserRepository<HTTPConnectionData>
    {

    }
}
