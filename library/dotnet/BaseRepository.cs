using Taskiea.Core.Connections;

namespace Taskiea.Core;

public abstract class BaseRepository : IRepository
{
    protected readonly IConnectionCache _connectionCache;

    public BaseRepository(IConnectionCache connectionCache)
    {
        ArgumentNullException.ThrowIfNull(connectionCache);
        _connectionCache = connectionCache;
    }

    public abstract void Initialize(string project);
}
