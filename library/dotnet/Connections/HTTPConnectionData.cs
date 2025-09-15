namespace Taskpiea.Core.Connections;

public class HTTPConnectionData : BaseConnectionData
{
    public HttpClient? HttpClient { get; set; }
    public string Endpoint { get; set; }

    public HTTPConnectionData(string projectName, string endpoint, HttpClient? httpClient = null) : base(projectName)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(endpoint);
        Endpoint = endpoint;

        // TODO: Should we support functions to create the client with more info than endpoint? Rigth now I dont plan auth so I doubt it.
        // If a more complicated connection is required, then set up the client externally.
        if (httpClient == null)
            httpClient = new HttpClient() { BaseAddress = new Uri(endpoint) };

        HttpClient = httpClient;
    }

    public override void Dispose()
    {
        HttpClient?.Dispose();
    }
}
