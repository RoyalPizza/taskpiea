
namespace Taskiea.Core.Accounts
{
    public class UserHTTPDataLayer : HTTPDataLayer<User>
    {
        public UserHTTPDataLayer(HttpClient httpClient) : base(httpClient)
        {

        }
    }
}
