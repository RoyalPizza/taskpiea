namespace Taskiea.Core.Connections;

public class HTTPConnectionData : BaseConnectionData
{
    public HttpClient? HttpClient { get; set; }
    public string Endpoint { get; set; }

    public HTTPConnectionData(string projectName, string endpoint, HttpClient? httpClient = null) : base(projectName)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(endpoint);
        Endpoint = endpoint;
        HttpClient = httpClient;
    }
}
