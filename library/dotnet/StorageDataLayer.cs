using System.Collections.Concurrent;
using Taskiea.Core.Results;

namespace Taskiea.Core;

/// <summary>
/// 
/// </summary>
/// <typeparam name="T"></typeparam>
/// <remarks>
/// The storage data liser is semi functional instead of full OOP.
/// That is because in the client server model, I do not want one
/// data layer object per layer per project. Becuse then I would have to manager
/// a list of data layers that grow infinitley based on the projects opened.
/// Instead, the functions will take in the connection information each time,
/// which lines up with our backend DB (Sqlite). I will cache connection strings,
/// but for now that is far less to manage than the idea of chached DL's.
/// </remarks>
public abstract class StorageDataLayer<T> : ICRUDDataLayer<T> where T : IDataObject
{
    private static readonly ConcurrentDictionary<string, string> _connectionStringCache = new ConcurrentDictionary<string, string>();

    protected static string GetConnectionString(ProjectConnectionData projectConnectionData)
    {
        if (_connectionStringCache.TryGetValue(projectConnectionData.ProjectName, out var cachedConnectionString))
            return cachedConnectionString;

        string connectionString;
        if (string.IsNullOrEmpty(projectConnectionData.Projectpath))
            connectionString = $"Data Source={projectConnectionData.ProjectName}.taskp";
        else
        {
            // TODO: validate path (e.g., check if directory exists, sanitize input)
            connectionString = $"Data Source={projectConnectionData.Projectpath}/{projectConnectionData.ProjectName}.taskp";
        }
        _connectionStringCache.TryAdd(projectConnectionData.ProjectName, connectionString);

        //

        return connectionString;
    }

    // this is so each datalayer can init their storage method (like make tables)
    public abstract void Initialize(ProjectConnectionData projectConnectionData);

    // standard CRUD ops
    public abstract Task<CreateResult<T>> CreateAsync(ProjectConnectionData projectConnectionData, T dataObject, CancellationToken cancellationToken);
    public abstract Task<DeleteResult> DeleteAsync(ProjectConnectionData projectConnectionData, uint id, CancellationToken cancellationToken);
    public abstract Task<UpdateResult<T>> UpdateAsync(ProjectConnectionData projectConnectionData, T dataObject, CancellationToken cancellationToken);

    // validation is seperate so the client can call it without doing the full operation
    public abstract Task<ValidateResult> ValidateCreateAsync(ProjectConnectionData projectConnectionData, T dataObject, CancellationToken cancellationToken);
    public abstract Task<ValidateResult> ValidateUpdateAsync(ProjectConnectionData projectConnectionData, T dataObject, CancellationToken cancellationToken);

    // TODO: The gets do not use result wrappers. Decide if this is good or bad
    public abstract Task<GetSingleResult<T>> GetSingleAsync(ProjectConnectionData projectConnectionData, uint id, CancellationToken cancellationToken);
    public abstract Task<GetManyResult<T>> GetAllAsync(ProjectConnectionData projectConnectionData, CancellationToken cancellationToken);
}
