namespace Taskpiea.Core.Connections;

public interface IConnectionCache
{
    void Register<TConnection>(TConnection connection) where TConnection : BaseConnectionData;
    void Unregister(string projectName);
    public void UnregisterAll();
    public T? GetConnectionData<T>(string projectName) where T : BaseConnectionData;
}
